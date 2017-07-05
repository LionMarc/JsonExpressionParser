namespace JsonExpressionParser
{
    using System;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;
    using Newtonsoft.Json.Linq;
    using Sprache;

    public class JsonExpressionParser<TContext> where TContext : JsonExpressionParserContext
    {
        #region Fields

        private readonly ParameterExpression currentContextExpression = Expression.Parameter(typeof(TContext));
        private readonly Parser<Expression> Expr;

        private readonly Parser<Expression> term;

        #endregion

        #region .ctor

        public JsonExpressionParser()
        {
            var number = Parse.DecimalInvariant
                .Select(x => Expression.Constant(double.Parse(x, CultureInfo.InvariantCulture)));
            var jsonInputField = Parse.Regex(@"\$(\.[a-zA-Z][a-zA-Z0-9]*)*")
                .Select(s => this.GenerateExpressionForJsonField(s));

            this.term = (from lparen in Parse.Char('(')
                         from expr in Parse.Ref(() => Expr)
                         from rparen in Parse.Char(')')
                         select expr).Named("expression")
                         .XOr(number)
                         .XOr(jsonInputField);

            this.Expr = Parse.ChainOperator(
                Operator("+", ExpressionType.AddChecked).Or(Operator("-", ExpressionType.SubtractChecked)),
                this.term,
                MakeBinary);
        }

        #endregion

        #region Public Methods

        public Func<TContext, TResultType> CreateFuncFromExpression<TResultType>(string expression)
        {
            return this.Expr.End()
                .Select(body => Expression.Lambda<Func<TContext, TResultType>>(
                    Expression.Convert(body, typeof(TResultType)),
                    this.currentContextExpression))
                .Parse(expression)
                .Compile();
        }

        #endregion

        #region Helper Methods

        private static Expression MakeBinary(ExpressionType type, Expression left, Expression right)
        {
            if (left.Type != typeof(object))
            {
                return Expression.MakeBinary(
                    type,
                    left,
                    Expression.Convert(right, left.Type));
            }

            return Expression.MakeBinary(
                type,
                Expression.Convert(left, right.Type),
                right);
        }

        private static Parser<ExpressionType> Operator(string op, ExpressionType opType)
        {
            return Parse.String(op).Token().Return(opType);
        }

        private static object GetJsonField(string jsonPath, TContext context)
        {
            return ((JValue)context.Current.SelectToken(jsonPath).Value<object>()).Value;
        }

        private Expression GenerateExpressionForJsonField(string jsonPath)
        {
            var methodInfo = this.GetType().GetMethod("GetJsonField", BindingFlags.Static | BindingFlags.NonPublic);
            return Expression.Call(
                methodInfo,
                Expression.Constant(jsonPath),
                this.currentContextExpression);
        }

        #endregion
    }
}
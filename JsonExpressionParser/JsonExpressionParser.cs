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
            var dateTime = Parse.Regex(@"[0-9]{4}-[0-9]{2}-[0-9]{2}")
                .Select(s => Expression.Constant(DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)));
            var number = Parse.DecimalInvariant
               .Select(x => Expression.Constant(double.Parse(x, CultureInfo.InvariantCulture)));
            var jsonInputField = Parse.Regex(@"\$(\.[a-zA-Z][a-zA-Z0-9]*)*")
                .Select(s => this.GenerateExpressionForJsonField(s));

            this.term = (from lparen in Parse.Char('(')
                         from expr in Parse.Ref(() => Expr)
                         from rparen in Parse.Char(')')
                         select expr).Named("expression")
                         .XOr(dateTime)
                         .XOr(number)
                         .XOr(jsonInputField);

            var innerExpr = Parse.ChainOperator(
                Operator("<", ExpressionType.LessThan),
                this.term,
                MakeBinary);

            innerExpr = Parse.ChainOperator(
                Operator("*", ExpressionType.MultiplyChecked).Or(Operator("/", ExpressionType.Divide)),
                innerExpr,
                MakeBinaryForDoubles);

            this.Expr = Parse.ChainOperator(
                Operator("+", ExpressionType.AddChecked).Or(Operator("-", ExpressionType.SubtractChecked)),
                innerExpr,
                MakeBinaryForDoubles);
        }

        #endregion

        #region Public Methods

        public Func<TContext, TResultType> CreateFuncFromExpression<TResultType>(string expression)
        {
            try
            {
                return this.Expr.End()
                    .Select(body => Expression.Lambda<Func<TContext, TResultType>>(Expression.Convert(body, typeof(TResultType)), this.currentContextExpression))
                    .Parse(expression)
                    .Compile();
            }
            catch (Exception e)
            {
                throw new JsonExpressionParserException($"Parsing error of the expression '{expression}'", e);
            }
        }

        #endregion

        #region Helper Methods

        private static Expression MakeBinaryForDoubles(ExpressionType type, Expression left, Expression right)
        {
            return Expression.MakeBinary(
                type,
                Expression.Convert(left, typeof(double)),
                Expression.Convert(right, typeof(double)));
        }

        private static Expression MakeBinary(ExpressionType type, Expression left, Expression right)
        {
            if (type == ExpressionType.LessThan)
            {
                return Expression.Condition(
                    Expression.LessThan(
                        Expression.Call(
                            Expression.Convert(left, typeof(IComparable)),
                            typeof(IComparable).GetMethod("CompareTo"),
                            Expression.Convert(right, typeof(IComparable))),
                        Expression.Constant(0)),
                    Expression.Constant(true),
                    Expression.Constant(false));
            }

            // TODO - Remove this code when all type of binary expression are implemented if this code is not useful
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
            var value = context.Current.SelectToken(jsonPath);
            if (value == null)
            {
                throw new JsonExpressionParserException($"The field with path '{jsonPath}' does not exist!");
            }

            return ((JValue)value.Value<object>()).Value;
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
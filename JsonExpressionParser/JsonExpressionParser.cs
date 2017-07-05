namespace JsonExpressionParser
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json.Linq;
    using Sprache;

    public class JsonExpressionParser<TContext> where TContext : JsonExpressionParserContext
    {
        #region Fields

        private readonly ParameterExpression currentContextExpression = Expression.Parameter(typeof(TContext));
        private readonly Parser<Expression> expression;
        private readonly List<Function> functions = new List<Function>();

        #endregion

        #region .ctor

        public JsonExpressionParser(params Function[] functions)
        {
            var dateTime = Parse.Regex(@"[0-9]{4}-[0-9]{2}-[0-9]{2}")
                .Select(s => Expression.Constant(DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)));
            var number = Parse.DecimalInvariant
               .Select(x => Expression.Constant(double.Parse(x, CultureInfo.InvariantCulture)));
            var @string = Parse.RegexMatch(@"'(.*)'").Select(s => Expression.Constant(s.Groups[1].Value));
            var jsonInputField = Parse.Regex(@"\$(\.[a-zA-Z][a-zA-Z0-9]*)*")
                .Select(s => this.GenerateExpressionForJsonField(s));
            var variable = Parse.RegexMatch(@"\$([a-zA-Z][a-zA-Z0-9]*)(\.[a-zA-Z][a-zA-Z0-9]*)*")
                .Select(s => this.GenerateExpressionForVariable(s));
            var functionCall =
                from name in Parse.Letter.AtLeastOnce().Text()
                from left in Parse.Char('(')
                from expressions in Parse.Ref(() => expression).DelimitedBy(Parse.Char(',').Token())
                from right in Parse.Char(')')
                select this.GenerateExpressionForFunctionCall(name, expressions.ToArray());

            var term = (from lparen in Parse.Char('(')
                        from expr in Parse.Ref(() => expression)
                        from rparen in Parse.Char(')')
                        select expr).Named("expression")
                         .XOr(dateTime)
                         .XOr(number)
                         .XOr(@string)
                         .XOr(variable)
                         .XOr(jsonInputField)
                         .XOr(functionCall);

            var innerExpr = Parse.ChainOperator(
                Operator("<", ExpressionType.LessThan),
                term,
                MakeBinary);

            innerExpr = Parse.ChainOperator(
                Operator("*", ExpressionType.MultiplyChecked).Or(Operator("/", ExpressionType.Divide)),
                innerExpr,
                MakeBinaryForDoubles);

            this.expression = Parse.ChainOperator(
                Operator("+", ExpressionType.AddChecked).Or(Operator("-", ExpressionType.SubtractChecked)),
                innerExpr,
                MakeBinaryForDoubles);

            this.functions.Add(new IF());
            this.functions.AddRange(functions);
        }

        #endregion

        #region Public Methods

        public Func<TContext, TResultType> CreateFuncFromExpression<TResultType>(string expression)
        {
            try
            {
                return this.expression.End()
                    .Select(body => Expression.Lambda<Func<TContext, TResultType>>(Expression.Convert(body, typeof(TResultType)), this.currentContextExpression))
                    .Parse(expression)
                    .Compile();
            }
            catch (Exception e) when (e.GetType() != typeof(JsonExpressionParserException))
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

        private static object GetVariable(string variableName, string jsonPath, TContext context)
        {
            var variable = context.Variables.FirstOrDefault(v => v.Name == variableName);
            if (variable == null)
            {
                throw new JsonExpressionParserException($"There is no variable with name '{variableName}' in the context.");
            }

            var value = variable.Value.SelectToken("$.value" + jsonPath);
            if (value == null)
            {
                throw new JsonExpressionParserException($"The variable '{variableName}' has no field with path '{jsonPath}'.");
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

        private Expression GenerateExpressionForFunctionCall(string functionName, Expression[] parameters)
        {
            var function = this.functions.Find(m => m.Name == functionName);
            if (function == null)
            {
                throw new JsonExpressionParserException($"There is no function with name '{functionName}'");
            }

            return function.CreateExpression(parameters);
        }

        private Expression GenerateExpressionForVariable(Match match)
        {
            var methodInfo = this.GetType().GetMethod("GetVariable", BindingFlags.Static | BindingFlags.NonPublic);
            return Expression.Call(
                methodInfo,
                Expression.Constant(match.Groups[1].Value),
                Expression.Constant(match.Groups[2].Value),
                this.currentContextExpression);
        }

        #endregion
    }
}
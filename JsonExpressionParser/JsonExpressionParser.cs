namespace JsonExpressionParser
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Newtonsoft.Json.Linq;
    using Sprache;

    public class JsonExpressionParserContext
    {
        public JsonExpressionParserContext(JObject current)
        {
            this.Current = current;
        }

        public JObject Current { get; private set; }
    }

    public class JsonExpressionParser<TContext> where TContext : JsonExpressionParserContext
    {
        #region Fields

        private readonly ParameterExpression currentContextExpression = Expression.Parameter(typeof(TContext));
        private readonly Parser<Expression> jsonInputField;

        #endregion

        #region .ctor

        public JsonExpressionParser()
        {
            this.jsonInputField = Parse.Regex(@"\$(\.[a-zA-Z][a-zA-Z0-9]*)*")
                .Select(s => this.GenerateExpressionForJsonField(s));
        }

        #endregion

        #region Public Methods

        public Func<TContext, TResultType> CreateFuncFromExpression<TResultType>(string expression)
        {
            return this.jsonInputField.End()
                .Select(body => Expression.Lambda<Func<TContext, TResultType>>(
                    Expression.Convert(body, typeof(TResultType)),
                    this.currentContextExpression))
                .Parse(expression)
                .Compile();
        }

        #endregion

        #region Helper Methods

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
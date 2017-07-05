namespace JsonExpressionParser
{
    using System.Linq.Expressions;

    public class IF : Function
    {
        public IF() : base("IF")
        {
        }

        public override Expression CreateExpression(Expression[] parameters)
        {
            if (parameters.Length != 3)
            {
                throw new JsonExpressionParserException("The IF function takes 3 parameters!");
            }

            if (parameters[1].Type != typeof(object))
            {
                return Expression.Condition(
                    parameters[0],
                    parameters[1],
                    Expression.Convert(parameters[2], parameters[1].Type));
            }

            if (parameters[2].Type != typeof(object))
            {
                return Expression.Condition(
                    parameters[0],
                    Expression.Convert(parameters[1], parameters[2].Type),
                    parameters[2]);
            }

            return Expression.Condition(
                parameters[0],
                parameters[1],
                parameters[2]);
        }
    }
}
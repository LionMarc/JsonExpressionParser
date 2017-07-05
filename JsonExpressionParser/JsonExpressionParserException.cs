namespace JsonExpressionParser
{
    using System;

    public class JsonExpressionParserException : Exception
    {
        public JsonExpressionParserException(string message) :
            base(message)
        {
        }

        public JsonExpressionParserException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }
}
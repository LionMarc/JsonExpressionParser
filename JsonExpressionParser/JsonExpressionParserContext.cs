namespace JsonExpressionParser
{
    using Newtonsoft.Json.Linq;

    public class JsonExpressionParserContext
    {
        public JsonExpressionParserContext(JObject current)
        {
            this.Current = current;
        }

        public JObject Current { get; private set; }
    }
}
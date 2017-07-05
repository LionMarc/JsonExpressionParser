namespace JsonExpressionParser
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    public class Variable
    {
        public Variable(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            this.Name = name;
            this.Value = JObject.FromObject(new VariableValue { Value = value });
        }

        public string Name { get; }

        public JObject Value { get; }

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        private class VariableValue
        {
            public Object Value { get; set; }
        }
    }
}
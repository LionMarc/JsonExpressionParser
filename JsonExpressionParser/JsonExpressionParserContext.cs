namespace JsonExpressionParser
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;

    public class JsonExpressionParserContext
    {
        private readonly List<Variable> variables = new List<Variable>();

        public JsonExpressionParserContext(JObject current)
        {
            this.Current = current;
        }

        public JObject Current { get; private set; }

        public IEnumerable<Variable> Variables => this.variables.AsReadOnly();

        public void AddVariable(string name, object value)
        {
            if (this.variables.Find(v => v.Name == name) != null)
            {
                throw new ArgumentException($"There is already a variable with name '{name}'");
            }

            this.variables.Add(new Variable(name, value));
        }
    }
}
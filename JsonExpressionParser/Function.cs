namespace JsonExpressionParser
{
    using System;
    using System.Linq.Expressions;

    public abstract class Function
    {
        protected Function(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            this.Name = name;
        }

        public string Name { get; }

        public abstract Expression CreateExpression(Expression[] parameters);
    }
}
﻿namespace JsonExpressionParser
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json.Linq;

    [TestClass]
    public class JsonExpressionParserTest
    {
        #region Fields

        private JObject inputData;

        #endregion

        #region SetUp/TearDown

        [TestInitialize]
        public void SetUp()
        {
            var jsonData = @"
{
    'date':'2017-07-04T00:00:00Z',
    'code':'code001',
    'currency':'EUR',
    'realAskPrice':123.456,
    'realBidPrice':118.987,
    'nested':{
        'string':'StringType',
        'double':4.56,
        'dateTime':'2017-05-08T02:00:00Z'
    }
}
";

            this.inputData = JObject.Parse(jsonData);
        }

        #endregion

        #region Tests

        [TestMethod]
        public void Should_parse_string_simple_field()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.currency";
            var func = jsonExpressionParser.CreateFuncFromExpression<string>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual("EUR", result);
        }

        [TestMethod]
        public void Should_parse_double_simple_field()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.realAskPrice";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(123.456, result);
        }

        [TestMethod]
        public void Should_parse_dateTime_simple_field()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.date";
            var func = jsonExpressionParser.CreateFuncFromExpression<DateTime>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(new DateTime(2017, 7, 4, 0, 0, 0, DateTimeKind.Utc), result);
        }

        [TestMethod]
        public void Should_parse_string_nested_field()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.nested.string";
            var func = jsonExpressionParser.CreateFuncFromExpression<string>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual("StringType", result);
        }

        [TestMethod]
        public void Should_parse_double_nested_field()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.nested.double";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(4.56, result);
        }

        [TestMethod]
        public void Should_parse_dateTime_nested_field()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.nested.dateTime";
            var func = jsonExpressionParser.CreateFuncFromExpression<DateTime>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(new DateTime(2017, 5, 8, 2, 0, 0, DateTimeKind.Utc), result);
        }

        #endregion
    }
}
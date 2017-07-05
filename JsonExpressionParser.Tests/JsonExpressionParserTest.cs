namespace JsonExpressionParser
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
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
    'effectiveDate':'2017-07-05T00:00:00Z',
    'code':'code001',
    'currency':'EUR',
    'realAskPrice':123.456,
    'realBidPrice':118.987,
    'forex': 1.23,
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

        #region Tests for nominal cases

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

        [TestMethod]
        public void Should_sum_the_double_field_with_the_numeric_constant_when_constant_is_at_right()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.realAskPrice + 4.0";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(127.456, result, 1E-10);
        }

        [TestMethod]
        public void Should_sum_the_double_field_with_the_numeric_constant_when_constant_is_at_left()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "5.5 + $.realAskPrice";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(128.956, result, 1E-10);
        }

        [TestMethod]
        public void Should_sum_the_two_double_fields()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.realBidPrice + $.realAskPrice";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(242.443, result, 1E-10);
        }

        [TestMethod]
        public void Should_substract_the_constant_value_from_the_double_field()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.realAskPrice - 4.0";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(119.456, result, 1E-10);
        }

        [TestMethod]
        public void Should_substract_the_double_field_from_the_constant_value()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "142 - $.realAskPrice";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(18.544, result, 1E-10);
        }

        [TestMethod]
        public void Should_substract_the_two_double_fields()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.realAskPrice - $.realBidPrice";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(4.469, result, 1E-10);
        }

        [TestMethod]
        public void Should_multiply_the_double_field_by_the_constant_value_when_constant_is_at_right()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.realAskPrice * 2.5";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(308.64, result, 1E-10);
        }

        [TestMethod]
        public void Should_multiply_the_double_field_by_the_constant_value_when_constant_is_at_left()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "2.5 * $.realAskPrice";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(308.64, result, 1E-10);
        }

        [TestMethod]
        public void Should_multiply_the_two_double_fields()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.realAskPrice * $.forex";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(151.85088, result, 1E-10);
        }

        [TestMethod]
        public void Should_divide_the_double_field_by_the_constant_value_when_constant_is_at_right()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.realAskPrice / 2.5";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(49.3824, result, 1E-10);
        }

        [TestMethod]
        public void Should_divide_the_double_field_by_the_constant_value_when_constant_is_at_left()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "2.5 / $.realAskPrice";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(0.0202501296, result, 1E-10);
        }

        [TestMethod]
        public void Should_divide_the_two_double_fields()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.realAskPrice / $.forex";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(100.3707317073, result, 1E-10);
        }

        [TestMethod]
        public void Should_apply_multiplication_before_addition()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "4 + 3 * 2";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(10.0, result, 1E-10);
        }

        [TestMethod]
        public void Should_apply_multiplication_after_addition_when_addition_is_inside_parenthesis()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "(4 + 3) * 2";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(14.0, result, 1E-10);
        }

        [TestMethod]
        public void Should_apply_division_before_addition()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "4 + 3 / 2";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(5.5, result, 1E-10);
        }

        [TestMethod]
        public void Should_apply_division_after_addition_when_addition_is_inside_parenthesis()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "(4 + 3) / 2";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(3.5, result, 1E-10);
        }

        [TestMethod]
        public void Should_apply_multiplication_before_substraction()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "4 - 3 * 2";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(-2.0, result, 1E-10);
        }

        [TestMethod]
        public void Should_apply_multiplication_after_substraction_when_substraction_is_inside_parenthesis()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "(4 - 3) * 2";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(2.0, result, 1E-10);
        }

        [TestMethod]
        public void Should_apply_division_before_substraction()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "4 - 3 / 2";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(2.5, result, 1E-10);
        }

        [TestMethod]
        public void Should_apply_division_after_substraction_when_substraction_is_inside_parenthesis()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "(4 - 3) / 2";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(0.5, result, 1E-10);
        }

        [TestMethod]
        public void Should_return_true_when_double_field_is_less_than_constant_number()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.realAskPrice < 130.0";
            var func = jsonExpressionParser.CreateFuncFromExpression<bool>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Should_return_false_when_double_field_is_greater_than_constant_number()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.realAskPrice < 110.0";
            var func = jsonExpressionParser.CreateFuncFromExpression<bool>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Should_return_true_when_constant_number_is_less_than_double_field()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "110.0 < $.realAskPrice";
            var func = jsonExpressionParser.CreateFuncFromExpression<bool>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Should_return_false_when_constant_number_is_greater_than_double_field()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "130.0 < $.realAskPrice";
            var func = jsonExpressionParser.CreateFuncFromExpression<bool>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Should_return_true_when_datetime_field_is_less_than_constant_datetime()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.date < 2017-07-05";
            var func = jsonExpressionParser.CreateFuncFromExpression<bool>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Should_return_false_when_datetime_field_is_greater_than_constant_datetime()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.date < 2017-07-03";
            var func = jsonExpressionParser.CreateFuncFromExpression<bool>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Should_return_true_when_constant_datetime_is_less_than_datetime_field()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "2017-07-03 < $.date";
            var func = jsonExpressionParser.CreateFuncFromExpression<bool>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Should_return_false_when_constant_datetime_is_greater_than_datetime_field()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "2017-07-05 < $.date";
            var func = jsonExpressionParser.CreateFuncFromExpression<bool>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Should_return_true_when_left_double_field_is_less_than_right_double_field()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.realBidPrice < $.realAskPrice";
            var func = jsonExpressionParser.CreateFuncFromExpression<bool>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Should_return_false_when_left_double_field_is_greater_than_right_double_field()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.realAskPrice < $.realBidPrice";
            var func = jsonExpressionParser.CreateFuncFromExpression<bool>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Should_return_true_when_left_datetime_field_is_less_than_right_datetime_field()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.date < $.effectiveDate";
            var func = jsonExpressionParser.CreateFuncFromExpression<bool>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Should_return_false_when_left_datetime_field_is_greater_than_right_datetime_field()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.effectiveDate < $.date";
            var func = jsonExpressionParser.CreateFuncFromExpression<bool>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Should_return_first_item_when_calling_IF_function_with_a_condition_evaluated_to_true()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "IF($.date < 2017-07-05,69.3, $.realAskPrice)";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(69.3, result, 1E-10);
        }

        [TestMethod]
        public void Should_return_second_item_when_calling_IF_function_with_a_condition_evaluated_to_false()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "IF($.date < 2017-07-03,69.3, $.realAskPrice)";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(123.456, result, 1E-10);
        }

        [TestMethod]
        public void Should_execute_the_userdefined_pow_function_when_registered_into_the_parser()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>(new Pow());

            var expression = "Pow(7, 2)";
            var func = jsonExpressionParser.CreateFuncFromExpression<double>(expression);

            var context = new JsonExpressionParserContext(this.inputData);

            var result = func(context);

            Assert.AreEqual(49.0, result, 1E-10);
        }

        [TestMethod]
        public void Should_parse_string_variable()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$x";
            var func = jsonExpressionParser.CreateFuncFromExpression<string>(expression);

            var context = new JsonExpressionParserContext(this.inputData);
            context.AddVariable("x", "testString");

            var result = func(context);

            Assert.AreEqual("testString", result);
        }

        #endregion

        #region Errors Tests

        [TestMethod]
        public void Should_throw_a_JsonExpressionParserException_when_requested_field_does_not_exist()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.priceCurrency";
            var func = jsonExpressionParser.CreateFuncFromExpression<string>(expression);

            var context = new JsonExpressionParserContext(this.inputData);
            var exception = Assert.ThrowsException<JsonExpressionParserException>(() => func(context));
            Assert.AreEqual("The field with path '$.priceCurrency' does not exist!", exception.Message);
        }

        [TestMethod]
        public void Should_throw_a_JsonExpressionParserException_when_trying_to_add_a_number_to_a_string()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "$.currency + 4.0";

            var exception = Assert.ThrowsException<JsonExpressionParserException>(() => jsonExpressionParser.CreateFuncFromExpression<string>(expression));

            Assert.AreEqual("Parsing error of the expression '$.currency + 4.0'", exception.Message);
        }

        [TestMethod]
        public void Should_throw_a_JsonExpressionParserException_when_using_a_not_defined_function()
        {
            var jsonExpressionParser = new JsonExpressionParser<JsonExpressionParserContext>();

            var expression = "ApplyForex($.realAskPrice,$.currency,'USD')";

            var exception = Assert.ThrowsException<JsonExpressionParserException>(() => jsonExpressionParser.CreateFuncFromExpression<string>(expression));

            Assert.AreEqual("There is no function with name 'ApplyForex'", exception.Message);
        }

        #endregion

        #region Nested Classes

        private class Pow : Function
        {
            public Pow() : base("Pow")
            {
            }

            public override Expression CreateExpression(Expression[] parameters)
            {
                return Expression.Call(
                    typeof(Math).GetMethod("Pow", BindingFlags.Static | BindingFlags.Public),
                    parameters);
            }
        }

        #endregion
    }
}
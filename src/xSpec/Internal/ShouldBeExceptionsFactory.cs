using System;

namespace xSpec.Internal
{
	public class ShouldBeExceptionsFactory
	{
		public static ShouldBeException create_should_be_string_exception(string expected, string actual)
		{
			var message = create_expected_vs_actual(string.Format("\"{0}\'", expected), string.Format("\"{0}\'", actual));
			return new ShouldBeException(message);
		}

		public static ShouldBeException create_should_be_short_exception(short expected, short actual)
		{
			var message = create_expected_vs_actual(expected.ToString(), actual.ToString());
			return new ShouldBeException(message);
		}

		public static ShouldBeException create_should_be_int_exception(int expected, int actual)
		{
			var message = create_expected_vs_actual(expected.ToString(), actual.ToString());
			return new ShouldBeException(message);
		}

		public static ShouldBeException create_should_be_decimal_exception(decimal expected, decimal actual)
		{
			var message = create_expected_vs_actual(expected.ToString(), actual.ToString());
			return new ShouldBeException(message);
		}

		public static Exception create_should_be_float_exception(float expected, float actual)
		{
			var message = create_expected_vs_actual(expected.ToString(), actual.ToString());
			return new ShouldBeException(message);
		}

		public static ShouldBeException create_should_be_bool_exception(bool expected, bool actual)
		{
			var message = create_expected_vs_actual(expected.ToString().ToLower(), actual.ToString().ToLower());
			return new ShouldBeException(message);
		}

		public static ShouldBeException create_should_start_with_string_exception(string expected, string actual)
		{
			var actuallyEndsWith = actual.Substring(actual.Length - expected.Length, expected.Length);

			var message = string.Concat(
				string.Format("\"{0}\"", expected),
				string.Concat("should end with", string.Format("\"{0}\"", actual), " but actually ends with "),
				string.Format("\"{0}\"", actuallyEndsWith));

			return new ShouldBeException(message);
		}

		public static Exception create_should_be_null_exception(object actual)
		{
			var message = create_expected_vs_actual("null", actual.ToString());
			return new ShouldBeException(message);
		}

		public static Exception create_should_not_be_null_exception(object actual)
		{
			var message = create_expected_vs_actual("not null", actual.ToString());
			return new ShouldBeException(message);
		}

		public static Exception create_should_be_datetime_exception(DateTime expected, DateTime actual)
		{
			return create_should_be_string_exception(expected.ToString(), actual.ToString());
		}

		public static Exception should_be_greater_than_exception(int expected, int actual)
		{
			var message = string.Format("{0} should be greater than {1}", expected, actual);
			return new ShouldBeException(message);
		}

		public static Exception should_be_less_than_exception(int expected, int actual)
		{
			var message = string.Format("{0} should be less than {1}", expected, actual);
			return new ShouldBeException(message);
		}

		private static string create_expected_vs_actual(string expected, string actual)
		{
			return string.Format("Expected: {0}, Actual: {1}", expected, actual);
		}
	}
}
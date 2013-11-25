using System;
using xSpec.Internal;

namespace xSpec
{
	public static class ShouldBeExtentions
	{
		public static void should_be(this string item, string value)
		{
			if (item.Equals(value) == false)
				throw ShouldBeExceptionsFactory.create_should_be_string_exception(value, item);
		}

		public static void should_be(this int item, int value)
		{
			if (item.Equals(value) == false)
				throw ShouldBeExceptionsFactory.create_should_be_int_exception(value, item);
		}

		public static void should_be(this short item, short value)
		{
			if (item.Equals(value) == false)
				throw ShouldBeExceptionsFactory.create_should_be_short_exception(value, item);
		}

		public static void should_be(this decimal item, decimal value)
		{
			if (item.Equals(value) == false)
				throw_should_be_exception(() => ShouldBeExceptionsFactory.create_should_be_decimal_exception(value, item));
		}

		public static void should_be(this float item, float value)
		{
			if (item.Equals(value) == false)
				throw_should_be_exception(() => ShouldBeExceptionsFactory.create_should_be_float_exception(value, item));
		}

		public static void should_be(this DateTime item, DateTime value)
		{
			if (item.Equals(value) == false)
				throw_should_be_exception(() => ShouldBeExceptionsFactory.create_should_be_datetime_exception(value, item));
		}

		public static void should_be_greater_than(this int expected, int actual)
		{
			if (expected < actual)
				throw_should_be_exception(() => ShouldBeExceptionsFactory.should_be_greater_than_exception(expected, actual));
		}

		public static void should_be_less_than(this int expected, int actual)
		{
			if (expected > actual)
				throw_should_be_exception(() => ShouldBeExceptionsFactory.should_be_less_than_exception(expected, actual));
		}

		public static void should_be_null(this object item)
		{
			if (item != null)
				throw_should_be_exception(() => ShouldBeExceptionsFactory.create_should_be_null_exception(item));
		}

		public static void should_not_be_null(this object item)
		{
			if (item == null)
				throw_should_be_exception(() => ShouldBeExceptionsFactory.create_should_not_be_null_exception(item));
		}

		public static void should_be_true(this bool item)
		{
			if (item != true)
				throw_should_be_exception(() => ShouldBeExceptionsFactory.create_should_be_bool_exception(true, false));
		}

		public static void should_be_false(this bool item)
		{
			if (item != false)
				throw_should_be_exception(() => ShouldBeExceptionsFactory.create_should_be_bool_exception(false, true));
		}

		public static void should_contain(this string item, string value)
		{
			if (item.Contains(value) == false)
			{
				//throw new ShouldBeExpectedNotMatchingActualException(item, "should contain", value);
			}
		}

		public static void should_start_with(this string item, string value)
		{
			if (item.StartsWith(value) == false)
			{
				throw_should_be_exception(() => ShouldBeExceptionsFactory.create_should_start_with_string_exception(value, item));
			}
		}

		public static void should_end_with(this string item, string value)
		{
			if (item.EndsWith(value) == false)
			{
				var actuallyEndsWith = item.Substring(item.Length - value.Length, value.Length);
				//throw new ShouldBeExpectedNotMatchingActualException(string.Format("\"{0}\"", item),
				//                                                     string.Concat("should end with", string.Format("\"{0}\"", value), " but actually ends with "),
				//                                                     string.Format("\"{0}\"", actuallyEndsWith));
			}
		}

		private static void throw_should_be_exception(Func<Exception> exception)
		{
			if(exception == null) return;

			Exception theException = exception();

			if (theException != null)
				throw theException;
		}
	}
}
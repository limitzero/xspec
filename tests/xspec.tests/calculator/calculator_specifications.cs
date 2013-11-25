using xSpec;

namespace xspec.sample.tests.calculator
{
	// subject under test:
	public class Calculator
	{
		private readonly int first_term;
		private readonly int second_term;

		public Calculator(int firstTerm, int secondTerm)
		{
			first_term = firstTerm;
			second_term = secondTerm;
		}

		public int add()
		{
			return first_term + second_term;
		}

		public int substract()
		{
			return first_term - second_term;
		}
		
		public int multiply()
		{
			return first_term * second_term;
		}
		
		public int divide()
		{
			return first_term / second_term;
		}
	}

	public class calculator_specifications : specification
	{
		private int first_term;
		private int second_term;
		private Calculator calculator;

		private void when_adding_two_non_negative_numbers()
		{
			int result = 0;

			before_each = () =>
			{
				first_term = 1;
				second_term = 2;
				calculator = new Calculator(first_term, second_term);
			};

			act = () =>
			{
				result = calculator.add();
			};

			it["should return the result of both terms added together"] = () =>
			{
				result.should_be(3);
			};

			it["should return a positive result from the addition"] = () =>
			{
				result.should_be_greater_than(0);
			};
		}

		private void specify_substracting_two_non_negative_numbers() // current scope
		{
			int result = 0;

			// "act" happens just before all "it" test conditions 
			// and after "establish" conditions in the current scope
			act = () =>
			{
				result = calculator.substract();
			};

			context["and the first term is less than the second term"] = () =>
			{
				establish = () =>
				{
					first_term = 1;
					second_term = 2;
					calculator = new Calculator(first_term, second_term);
				};

				it["should return a negative number"] = () =>
				{
					result.should_be(-1);
				};				
			};
		}
	}
}
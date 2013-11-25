using System;

namespace xspec.runner.utility.Result
{
	public interface ISpecificationResult
	{
		/// <summary>
		/// Gets or sets the number of specification scenarios
		/// </summary>
		int number_of_examples { get; set; }

		/// <summary>
		/// Gets or sets  the number of test conditions inside of the specification.
		/// </summary>
		int number_of_conditions { get; set; }

		/// <summary>
		/// Gets or sets the number of pending test conditions inside of the specification.
		/// </summary>
		int number_of_pending_conditions { get; set; }

		/// <summary>
		/// Gets or sets the number of passing test conditions inside of the specification.
		/// </summary>
		int number_of_passed_conditions { get; set; }

		/// <summary>
		/// Gets or sets the number of failing test conditions inside of the specification.
		/// </summary>
		int number_of_failed_conditions { get; set; }

		/// <summary>
		/// Gets or sets the verbalized specification for review.
		/// </summary>
		string verbalized_specification { get; set; }

		/// <summary>
		/// Gets the full failure message from the stack lines of the generated exception
		/// </summary>
		string failure_message { get; }

		/// <summary>
		/// Gets or sets the time in seconds for the specification to execute:
		/// </summary>
		decimal duration { get; set; }

		/// <summary>
		/// Gets the condition that caused the specification test failure
		/// </summary>
		string[] failure_conditions { get; }

		string[] stack_lines { get; }

		void create_failure_message(string message);
	}
}
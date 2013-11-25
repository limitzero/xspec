using xspec.runner.utility.Runner;
using Xunit;

namespace xunit.xSpec.runner
{
	/// <summary>
	/// Base class that "observes" or sets-up the basic infrastructure for the subject 
	/// under test for interaction with its collaborators.
	/// </summary>
	/// <typeparam name="TSubjectUnderTest">Current class under test with its collaborators</typeparam>
	public abstract class observes<TSubjectUnderTest> :
		global::xSpec.Mocking.observes<TSubjectUnderTest> where TSubjectUnderTest : class
	{
		private readonly ISpecificationRunner runner;

		protected observes()
		{
			this.runner = new SpecificationRunner();
		}

		[Fact]
		public void run()
		{
			// let xUnit know when the specification runner fails on executing a specification:
			runner.OnRunnerFailure += (runner_exception) => { throw runner_exception; };
			runner.RunInstance(this);
		}
	}
}
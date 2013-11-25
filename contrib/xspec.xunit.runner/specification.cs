using xspec.runner.utility.Runner;
using Xunit;

namespace xunit.xSpec.runner
{
	public abstract class specification : global::xSpec.specification
	{
		private readonly ISpecificationRunner runner;

		protected specification()
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

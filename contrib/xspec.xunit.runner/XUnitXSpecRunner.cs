using System;
using Xunit;

namespace xunit.xSpec.runner
{
	/// <summary>
	/// This is the extension of xUnit to rull all of the specification tests 
	/// </summary>
	public class XUnitXSpecRunner<TRunner> where TRunner : class
	{
		[Fact]
		public void RunAll()
		{
			var specification_runner = new xspec.runner.utility.Runner.SpecificationRunner();
			specification_runner.RunAssembly(typeof(TRunner).Assembly);
		}
	}
}
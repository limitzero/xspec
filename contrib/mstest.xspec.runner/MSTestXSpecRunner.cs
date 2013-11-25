using Microsoft.VisualStudio.TestTools.UnitTesting;
using xspec.runner.utility.Runner;

namespace mstest.xspec.runner
{
	[TestClass]
	public class MSTestXSpecRunner<TRunner> where TRunner : class
	{
		[TestMethod]
		public void RunAll()
		{
			var specification_runner = new SpecificationRunner();
			specification_runner.RunAssembly(typeof(TRunner).Assembly);
		}
	}
}
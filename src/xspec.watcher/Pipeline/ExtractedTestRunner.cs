using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using xspec.runner.utility;
using xspec.runner.utility.Result;
using xspec.runner.utility.Runner;

namespace xSpec.watcher.Pipeline
{
	public class ExtractedTestRunner : IExtractedTestRunner
	{
		private readonly ISpecificationRunner runner;

		public ExtractedTestRunner()
		{
			this.runner = new SpecificationRunner();
		}

		public void Run(IEnumerable<string> testLibraries)
		{
			var resolver = new TypeResolver();

			foreach (var testLibrary in testLibraries)
			{
				Assembly assembly = null;
				Exception loadAssemblyException;

				if(resolver.TryLoadAssemblyFromFile(testLibrary, out assembly, out loadAssemblyException) == false)
					continue;

				Console.WriteLine("running tests for assembly : " + testLibrary);
				runner.RunAssembly(assembly);
				assembly = null;
				Console.WriteLine();
			}
		}
	}
}
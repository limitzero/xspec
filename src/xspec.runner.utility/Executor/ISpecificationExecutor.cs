using System.Collections.Generic;
using xspec.runner.utility.Result;
using xspec.runner.utility.Settings;

namespace xspec.runner.utility.Executor
{
	public interface ISpecificationExecutor
	{
		IEnumerable<SpecificationResult> Execute(RunSettings settings);
	}
}
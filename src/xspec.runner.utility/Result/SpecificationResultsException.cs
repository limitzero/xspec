using System;
using System.Collections.Generic;
using System.Linq;

namespace xspec.runner.utility.Result
{
	public class SpecificationResultException : Exception
	{
		public IEnumerable<ISpecificationResult> SpecificationResults { get; private set; }
		public IEnumerable<string> FailureConditions { get; set; }
		public IEnumerable<string> StackLines { get; private set; }

		public SpecificationResultException(IEnumerable<ISpecificationResult> results)
			: base(CreateSpecificationResultsMessage(results))
		{
			this.SpecificationResults = results;
			this.StackLines = new List<string>(results.SelectMany(r => r.stack_lines));
			this.FailureConditions = new List<string>(results.SelectMany(r => r.failure_conditions));
		}

		private static string CreateSpecificationResultsMessage(IEnumerable<ISpecificationResult> results)
		{
			var message = string.Empty;
			foreach (var result in results)
			{
				message += string.Concat(result.failure_message, System.Environment.NewLine);
			}
			
			return message;
		}
	}
}

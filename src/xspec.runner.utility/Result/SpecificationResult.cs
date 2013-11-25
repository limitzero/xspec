using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xspec.runner.utility.Result
{
	public class SpecificationResult : ISpecificationResult
	{
		private readonly StringBuilder messages = new StringBuilder();
		private readonly IList<string> failing_conditions = new List<string>();

		public int number_of_examples { get; set; }
		public int number_of_conditions { get; set; }
		public int number_of_pending_conditions { get; set; }
		public int number_of_passed_conditions { get; set; }
		public int number_of_failed_conditions { get; set; }
		public string verbalized_specification { get; set; }

		public string[] failure_conditions
		{
			get { return failing_conditions.ToArray(); }
		}

		public string failure_message { get { return messages.ToString(); } }

		public string[] stack_lines
		{
			get
			{
				var lines = messages.ToString().
					Split(new string[] { System.Environment.NewLine },
							StringSplitOptions.RemoveEmptyEntries);
				return lines;
			}
		}

		public decimal duration { get; set; }

		public void create_failure_message(string message)
		{
			messages.AppendLine(message);
			var condition = message.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries)[0];
			failing_conditions.Add(condition);
		}
	}
}
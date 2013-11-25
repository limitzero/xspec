using System.Text;

namespace xSpec.Processor
{
	public class specification_processor_result
	{
		private StringBuilder failure_messages = new StringBuilder();

		public int number_of_examples { get; set; }
		public int number_of_conditions { get; set; }
		public int number_of_pending_conditions { get; set; }
		public int number_of_passed_conditions { get; set; }
		public int number_of_failed_conditions { get; set; }
		public string verbalized_specification { get; set; }
		public string failure_message { get { return failure_messages.ToString(); } }
		public decimal duration { get; set; }

		public void create_failure_message(string message)
		{
			failure_messages.AppendLine(message);
		}
	}
}
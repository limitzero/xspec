using System.Text;
using xspec.console.Runner.CommandLine;

namespace xspec.console.Runner
{
	public class ExecutingOptions
	{
		[Argument(ArgumentType.AtMostOnce, HelpText = "File name or full path to test library)", ShortName = "library")]
		public string Library;

		public bool IsParsed
		{
			get
			{
				return string.IsNullOrEmpty(this.Library) == false;
			}
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append(" {library}");

			return sb.ToString();
		}
	}
}
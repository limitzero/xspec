using System.Reflection;
using System.Text;
using WPFGrowlNotification.Runner.CommandLine;

namespace WPFGrowlNotification.Runner
{
	public class ExecutingOptions
	{
		public Actions Action = Actions.Notify;

		[Argument(ArgumentType.AtMostOnce, HelpText = "Level (info, pass, fail, pending)", ShortName = "level")]
		public string Level;

		[Argument(ArgumentType.Required, HelpText = "Title", ShortName = "title")]
		public string Title;

		[Argument(ArgumentType.Required, HelpText = "Message", ShortName = "message")]
		public string Message;

		public bool IsParsed
		{
			get
			{
				return 
				       string.IsNullOrEmpty(this.Level) == false &&
				       string.IsNullOrEmpty(this.Title) == false &&
				       string.IsNullOrEmpty(this.Message) == false;
			}
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append(" /level:").Append(Level)
				.Append(" /title:")
				.Append(Title)
				.Append(" /message:")
				.Append(Message);

			return sb.ToString();
		}
	}
}
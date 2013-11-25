using System;
using System.Collections.Specialized;
using System.IO;

namespace WPFGrowlNotification.Modes
{
	public class HelpMode 
	{
		public Notification Execute(StringDictionary commands)
		{
			try
			{
				Console.Title = "xspec.console Help";

				Stream stream = typeof(Notification).Assembly
					.GetManifestResourceStream("xspec.console.Content.Help.txt");

				if (stream != null)
				{
					using (TextReader reader = new StreamReader(stream))
					{
						string usage = reader.ReadToEnd();
						Console.WriteLine(usage);
						Console.Read();
					}
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception);
				Console.ReadKey();
			}

			return null;
		}
	}
}
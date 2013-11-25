using System;
using System.Collections.Specialized;
using System.IO;
using xSpec.watcher;
using xSpec.watcher.Modes;

namespace xspec.watcher.Modes
{
	public class HelpMode : IRunMode
	{
		public void Execute(StringDictionary commands)
		{
			try
			{
				using (Stream stream = typeof(Program).Assembly
					.GetManifestResourceStream("xspec.console.Content.Help.txt"))
				{
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
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception);
				Console.ReadKey();
			}
		}
	}
}
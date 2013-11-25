using System;
using System.Collections.Generic;
using xspec.console.Modes;
using xspec.console.Runner;
using xspec.console.Runner.CommandLine;

namespace xspec.console
{
	class Program
	{
		private static readonly IDictionary<RunModes, IRunMode> runmodes = new Dictionary<RunModes, IRunMode>();

		static void Main(string[] args)
		{
			new ConsoleRunner().RunAssembly(args[0]);
		}

		private static void RunTestSession(string[] args)
		{
			InitializeRunModes();

			var executingOptions = new ExecutingOptions();

			if (Parser.ParseArguments(args, executingOptions) == false && executingOptions.IsParsed == false)
			{
				Console.WriteLine("Invalid arguments:");
				Console.WriteLine("\t{0}", string.Join(" ", args));
				Console.WriteLine();
				Console.WriteLine(Parser.ArgumentsUsage(typeof(ExecutingOptions)));
			}

			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

			try
			{
				RunInMode(executingOptions);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception);
				System.Environment.Exit(1);
			}
		}

		private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Exception ex = e.ExceptionObject as Exception;

			if (ex != null)
				Console.WriteLine(ex.ToString());
			else
				Console.WriteLine("Error of unknown type thrown in applicaton domain");

			Environment.Exit(1);
		}

		private static void InitializeRunModes()
		{
			runmodes.Add(RunModes.Library, new LibraryMode());
		}

		private static void RunInMode(ExecutingOptions executingOptions)
		{
			runmodes[RunModes.Library].Execute(executingOptions);
		}
	}
}

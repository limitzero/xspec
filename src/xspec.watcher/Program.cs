using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using xspec.watcher;
using xSpec.watcher.Internal;
using xspec.watcher.Modes;
using xSpec.watcher.Modes;
using xSpec.watcher.Pipeline;

namespace xSpec.watcher
{
	class Program
	{
		private static readonly IDictionary<RunModes, IRunMode> runmodes = new Dictionary<RunModes, IRunMode>();
		private static object locker = new object();
		private static ConcurrentDictionary<string, long> changed_files;
		private const int MAXIMUM_CHANGED_FILE_COUNT = 256;
		private bool isExecuting;

		static void Main(string[] args)
		{
			InitializeRunModes();
			StringDictionary commands = CommandLineParser.SplitArgString(args);
			var command = GetCommandOption(commands);

			try
			{
				if (string.IsNullOrEmpty(command) ||
					command == RunModes.Help.ToString().ToLower() ||
					command == "?")
				{
					RunInMode(RunModes.Help, commands);
				}
				else 
				{
					RunInMode(RunModes.Path, commands);
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception);
			}
		}

		private static void InitializeRunModes()
		{
			runmodes.Add(RunModes.Path, new PathMode());
		}

		private static string GetCommandOption(StringDictionary commands)
		{
			// command should be the first option on the command line
			// (make it "Help" by default):
			var keys = new ArrayList(commands.Keys);

			var command = RunModes.Path.ToString().ToLower();

			if (keys.Count > 0)
				command = keys[0] as string;

			if (string.IsNullOrEmpty(command))
				command = command.Trim();

			return command.ToLower().Replace(":", string.Empty);
		}

		private static void RunInMode(RunModes runMode, StringDictionary commands)
		{
			runmodes[runMode].Execute(commands);
		}

		private static string read_command_line_to_quit_application()
		{
			Console.WriteLine("xSpec file watcher started, press 'q' to quit");
			var entry = Console.ReadLine();
			return entry.ToUpper();
		}

		private FileSystemWatcher configure_file_watcher()
		{
			string path_to_watch = string.Empty;
			Exception extract_path_to_watch_exception = null;

			if (try_get_path_to_watch(out path_to_watch, out extract_path_to_watch_exception) == false)
			{
				throw new Exception("There was an error extracting the path to watch from the configuration file.", 
					extract_path_to_watch_exception);
			}

			Console.WriteLine(string.Concat(">> Watching path : ", path_to_watch));

			FileSystemWatcher watcher = new FileSystemWatcher(path_to_watch);
			watcher.Changed += OnFileChanged;
			watcher.EnableRaisingEvents = true;
			watcher.IncludeSubdirectories = true;
			watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size;

			return watcher;
		}

		private  void OnFileChanged(object sender, FileSystemEventArgs e)
		{
			FileInfo queued_file = null;
			enqueue_current_file_for_triggering_build(e.FullPath, out queued_file);

			if(queued_file != null)
			{
				string code_file_filters = string.Empty;
				Exception extract_code_filters_exception = null;

				if (try_get_code_filter_to_inspect(out code_file_filters, out extract_code_filters_exception) == false)
				{
					throw extract_code_filters_exception;
				}

				var filters = code_file_filters.Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries);
				if(filters.Any(f => queued_file.Extension.Equals(f.Replace("*", string.Empty))))
				{
					Console.WriteLine(string.Empty);
					Console.WriteLine("========================= xspec watcher  (starting session) =========================");
					DefaultPipeline pipeline = new DefaultPipeline();
					pipeline.Configure();

					pipeline.Execute(System.AppDomain.CurrentDomain.BaseDirectory, e.FullPath);
					Console.WriteLine("========================= xspec watcher  (ending session) ==========================");
					Console.WriteLine(string.Empty);
				}
			}
		}

		private void PipelineExecutionCompleted(IAsyncResult ar)
		{
			AsyncResult result = ar as AsyncResult;
			PipelineExecuteDelegate pipelineExecuteDelegate = result.AsyncDelegate as PipelineExecuteDelegate;
			pipelineExecuteDelegate.EndInvoke(ar);
		}

		private static void enqueue_current_file_for_triggering_build(string current_file, out FileInfo queued_file)
		{
			FileInfo file_to_enqueue = null;
			queued_file = file_to_enqueue;

			if(File.Exists(current_file) == false) return;

		    file_to_enqueue = new FileInfo(current_file);
			queued_file = file_to_enqueue;

			lock (locker)
			{
				if (changed_files.Count == MAXIMUM_CHANGED_FILE_COUNT)
				{
					changed_files = new ConcurrentDictionary<string, long>();
#if DEBUG
					Console.WriteLine(string.Concat(">> Internal cache recycled <<"));
#endif
				}

				// first time we have seen the file:
				if( changed_files.ContainsKey(current_file) == false)
				{
					changed_files.TryAdd(current_file, file_to_enqueue.Length);
				}
				// same file, content added or subtracted:
				else if (changed_files.ContainsKey(current_file) == true && 
					changed_files[current_file] != file_to_enqueue.Length)
				{
					changed_files.TryAdd(current_file, file_to_enqueue.Length);
				}
				// file in cache unchanged, do nothing:
				else
				{
					queued_file = null;
				}
			}
		}

		private static bool try_get_path_to_watch(out string path_to_watch, out Exception exception)
		{
			const string key = "watch.path";
			path_to_watch = string.Empty;
			exception = null;
			bool success = false;

			try
			{
				path_to_watch = System.Configuration.ConfigurationManager.AppSettings.Get(key);
				success = true;
			}
			catch (Exception extractWatchPathException)
			{
				exception = extractWatchPathException;
			}

			if (path_to_watch.Equals(".") == true)
			{
				path_to_watch = System.AppDomain.CurrentDomain.BaseDirectory;
			}

			if (string.IsNullOrEmpty(path_to_watch))
			{
				exception = new InvalidOperationException(string.Format("The path to watch was specified for configuration key '{0}'", key));
				success = false;
			}

			return success;
		}

		private static bool try_get_code_filter_to_inspect(out string code_file_filters, out Exception exception)
		{
			const string key = "code.file.extensions";
			exception = null;
			code_file_filters = string.Empty;
			bool success = false;

			try
			{
				code_file_filters = System.Configuration.ConfigurationManager.AppSettings.Get(key);
				success = true;
			}
			catch (Exception extractCodeFileFilters)
			{
				exception = extractCodeFileFilters;
			}

			return success;
		}

		private static void build_and_run()
		{
			// using PSake
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.FileName = @"powershell.exe";
			startInfo.Arguments = Path.Combine(System.Environment.CurrentDirectory, @"\psake.cmd'");
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow = true;
			Process process = new Process();
			process.StartInfo = startInfo;
			process.Start();

			string output = process.StandardOutput.ReadToEnd();
			Console.WriteLine(output);
		}

		private static void notifiy(string text)
		{

		}
	}


}

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using xSpec.watcher.Modes;
using xSpec.watcher.Pipeline;

namespace xspec.watcher
{
	public class ConsoleRunner
	{
		private static readonly object locker = new object();
		private static ConcurrentDictionary<string, long> changedFiles;
		private const int MaximumChangedFileCount = 256;
		private static string watchedPath;
		private static FileSystemWatcher watcher; 

		public ConsoleRunner()
		{
			changedFiles = new ConcurrentDictionary<string, long>();
		}

		public void RunPath(string pathName)
		{
			if(string.IsNullOrEmpty(pathName))
				throw new ArgumentNullException("path","The path where the test libraries are located was not specified.");

			// no directory specified, use the base directory of the current watcher directory to start looking:
			if (pathName.Equals("."))
			{
				pathName = System.AppDomain.CurrentDomain.BaseDirectory
					.Replace(@"\bin\Debug", string.Empty)
					.Replace(@"\bin\Release", string.Empty);
			}

			if(Directory.Exists(pathName) == false)
				throw new InvalidOperationException(string.Format("The directory '{0}' does not exist.", pathName));

			watchedPath = pathName;

			start_watching(watchedPath);
		}

		private static void start_watching(string pathName)
		{
			using (watcher = configure_file_watcher(pathName))
			using (var indexer = new Indexer(pathName))
			{
				while (read_command_line_to_quit() != "Q")
				{
					// look for files that have changed...
				}
				watcher.Changed -= OnFileChanged;
			}
		}

		private static string read_command_line_to_quit()
		{
			Console.WriteLine("xspec watcher started, press 'q' to quit");
			var entry = Console.ReadLine();
			return entry.ToUpper();
		}

		private static FileSystemWatcher configure_file_watcher(string pathName)
		{
			Console.WriteLine(string.Concat("watching path : ", pathName));
			FileSystemWatcher watcher = new FileSystemWatcher(pathName);
			watcher.Changed += OnFileChanged;
			watcher.EnableRaisingEvents = true;
			watcher.IncludeSubdirectories = true;
			watcher.NotifyFilter = NotifyFilters.LastWrite;
			return watcher;
		}

		private static void OnFileChanged(object sender, FileSystemEventArgs e)
		{
			FileInfo queued_file = null;
			EnqueueFileForTriggeringBuild(e.FullPath, out queued_file);

			if (queued_file != null)
			{
				string code_file_filters = string.Empty;
				string exclusions = string.Empty;
				Exception extract_exception = null;

				if (TryGetCodeFiltersForInspection(out code_file_filters, out extract_exception) == false)
				{
					throw extract_exception;
				}

				if (TryGetFiltersForExclusions(out exclusions, out extract_exception) == false)
				{
					throw extract_exception;
				}

				if (string.IsNullOrEmpty(exclusions) == false)
				{
					var exclusion_filters = exclusions.Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries);
					if (exclusion_filters.Any(f => queued_file.Extension.Equals(f.Replace("*", string.Empty))) ||
					    exclusion_filters.Any(f => queued_file.FullName.Contains(f.Replace("*", string.Empty))))
					{
						return;
					}
				}

				var filters = code_file_filters.Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries);
				if (filters.Any(f => queued_file.Extension.Equals(f.Replace("*", string.Empty))))
				{
					Console.WriteLine("file selected for triggering build and test session:" + e.FullPath);

					watcher.EnableRaisingEvents = false;
					watcher.Changed -= OnFileChanged;

					DefaultPipeline pipeline = new DefaultPipeline();
					pipeline.Configure();
					pipeline.Execute(watchedPath, e.FullPath);

					watcher.EnableRaisingEvents = true;
					watcher.Changed += OnFileChanged;
				}
			}
		}

		private static bool TryGetCodeFiltersForInspection(out string code_file_filters, out Exception exception)
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

		private static bool TryGetFiltersForExclusions(out string exclusions, out Exception exception)
		{
			const string key = "exclusions";
			exception = null;
			exclusions = string.Empty;
			bool success = false;

			try
			{
				exclusions = System.Configuration.ConfigurationManager.AppSettings.Get(key);
				success = true;
			}
			catch (Exception extractCodeFileFilters)
			{
				exception = extractCodeFileFilters;
			}

			return success;
		}

		private static void EnqueueFileForTriggeringBuild(string current_file, out FileInfo queued_file)
		{
			FileInfo fileToEnqueue = null;
			queued_file = fileToEnqueue;

			if (File.Exists(current_file) == false) return;

			fileToEnqueue = new FileInfo(current_file);
			queued_file = fileToEnqueue;

			lock (locker)
			{
				if (changedFiles.Count == MaximumChangedFileCount)
				{
					changedFiles = new ConcurrentDictionary<string, long>();
					Console.WriteLine(string.Concat(">> Internal cache recycled"));
				}

				// first time we have seen the file:
				if (changedFiles.ContainsKey(current_file) == false)
				{
					changedFiles.TryAdd(current_file, fileToEnqueue.Length);
				}
				// same file, content added or subtracted:
				else if (changedFiles.ContainsKey(current_file) == true &&
					changedFiles[current_file] != fileToEnqueue.Length)
				{
					changedFiles.TryAdd(current_file, fileToEnqueue.Length);
				}
				// file in cache unchanged, do nothing:
				else
				{
					queued_file = null;
				}
			}
		}
	}
}
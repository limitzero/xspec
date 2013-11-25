using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using watcher.core;

namespace watcher
{
	class Program
	{
		private static object locker = new object();
		private static ConcurrentBag<Tuple<string, long>> changed_files;
		private const int MAXIMUM_CHANGED_FILE_COUNT = 256;
		private static WatcherConnector connector;
		private static string path_to_watch;

		static void Main(string[] args)
		{
			Console.Title = "watcher - (CTRL+C or 'Q' to quit)";
			changed_files = new ConcurrentBag<Tuple<string, long>>();
			connector = new WatcherConnector();

			using (var watcher = configure_file_watcher())
			{
				extract_runtime_information();

				connector.Configure();

				while (read_command_line_to_quit_application() != "q")
				{
					// look for files that have changed...
				}

				watcher.Changed -= OnFileChanged;
			}
		}

		private static void extract_runtime_information()
		{
			string content = string.Empty;

			using (Stream stream = typeof(Program).Assembly.GetManifestResourceStream("watcher.info.txt"))
			using (StreamReader reader = new StreamReader(stream))
			{
				content = reader.ReadToEnd();
			}

			Console.WriteLine("======================== watcher started (press 'q' key to quit) ======================== ");

			if(string.IsNullOrEmpty(content) == false)
			{
				content = content.Replace("{watch.path}", path_to_watch);

				string unit_test_runner = string.Empty;
				Exception extract_unit_test_runner_exception;
				try_get_unit_test_runner(out unit_test_runner, out extract_unit_test_runner_exception);
				content = content.Replace("{unit.test.runner}", unit_test_runner);

				string code_extentions = string.Empty;
				Exception extract_code_extensions_exception;
				try_get_code_filter_to_inspect(out code_extentions, out extract_code_extensions_exception);
				content = content.Replace("{code.file.extensions}", code_extentions);
			}

			Console.WriteLine(content);
		}

		private static string read_command_line_to_quit_application()
		{
			var entry = Console.ReadLine();
			return entry.ToLower();
		}

		private static FileSystemWatcher configure_file_watcher()
		{
			Exception extract_path_to_watch_exception = null;

			if (try_get_path_to_watch(out path_to_watch, out extract_path_to_watch_exception) == false)
			{
				throw new Exception("There was an error extracting the path to watch from the configuration file.", extract_path_to_watch_exception);
			}

			FileSystemWatcher watcher = new FileSystemWatcher(path_to_watch);
			watcher.Changed += OnFileChanged;
			watcher.EnableRaisingEvents = true;
			watcher.IncludeSubdirectories = true;
			watcher.NotifyFilter = NotifyFilters.LastWrite;

			return watcher;
		}

		private static void OnFileChanged(object sender, FileSystemEventArgs e)
		{
			FileInfo queued_file = null;
			enqueue_current_file_for_triggering_build(e.FullPath, out queued_file);

			if (queued_file != null)
			{
				string code_file_filters = string.Empty;
				Exception extract_code_filters_exception = null;

				if (try_get_code_filter_to_inspect(out code_file_filters, out extract_code_filters_exception) == false)
				{
					throw extract_code_filters_exception;
				}

				var filters = code_file_filters.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
				if (filters.Any(f => queued_file.Extension.Equals(f.Trim().Replace("*", string.Empty))))
				{
					FileChangedEvent @event = new FileChangedEvent
												{
													ChangeType = e.ChangeType,
													Directory = queued_file.DirectoryName,
													File = queued_file.Name,
													WatchedDirectory = path_to_watch
												};

					connector.EventPublisher.Publish(@event);
				}
			}
		}

		private static void enqueue_current_file_for_triggering_build(string current_file, out FileInfo queued_file)
		{
			FileInfo file_to_enqueue = null;
			queued_file = null;

			if (File.Exists(current_file) == false) return;

			file_to_enqueue = new FileInfo(current_file);

			lock (locker)
			{
				if (changed_files.Count == MAXIMUM_CHANGED_FILE_COUNT)
				{
					changed_files = new ConcurrentBag<Tuple<string, long>>();
					Console.WriteLine(string.Concat(">> Internal cache recycled"));
				}

				var tuple = new Tuple<string, long>(current_file, file_to_enqueue.Length);

				// first time the file name and length tuple has been encountered:
				if (changed_files.Any(t => t.Equals(tuple)) == false)
				{
					changed_files.Add(tuple);
					queued_file = file_to_enqueue;
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


		private static bool try_get_unit_test_runner(out string unit_test_runner, out Exception exception)
		{
			const string key = "unit.test.runner";
			unit_test_runner = string.Empty;
			exception = null;
			bool success = false;

			try
			{
				unit_test_runner = System.Configuration.ConfigurationManager.AppSettings.Get(key);
				success = true;
			}
			catch (Exception extract_unit_test_runner_exception)
			{
				exception = extract_unit_test_runner_exception;
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
	}
}

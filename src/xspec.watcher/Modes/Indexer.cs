using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Timers;
using xSpec.watcher.Pipeline;
using Enumerable = System.Linq.Enumerable;

namespace xSpec.watcher.Modes
{
	public class Indexer : IDisposable
	{
		private readonly string solution_file_path;
		private Timer indexer_timer;
		private bool disposed;

		/// <summary>
		/// Gets the file path to the solution that is being monitored for changes in tests.
		/// </summary>
		public static string Solution { get; private set; }

		/// <summary>
		/// Gets the current index of project and class files for inspection for changes:
		/// </summary>
		public  static ConcurrentDictionary<string, Index> Indexes { get; private set; }

		/// <summary>
		/// Ctor. Initializes the indexer with a solution file
		/// </summary>
		/// <param name="solutionFilePath"></param>
		public Indexer(string solutionFilePath)
		{
			solution_file_path = solutionFilePath;
		
			if (Indexes == null)
				Indexes = new ConcurrentDictionary<string, Index>();

			this.indexer_timer = new Timer(2000);
			this.indexer_timer.Elapsed += OnIndexerTimerElapsed;
			this.indexer_timer.Start();
		}

		/// <summary>
		/// This will examine the solution file in the defined path and extract all projects and compile time objects 
		/// for inspection on changes.
		/// </summary>
		public void Index()
		{
			if(this.disposed) return;

			var solution = Enumerable.FirstOrDefault<string>(Directory.GetFiles(Path.Combine(this.solution_file_path))
				                        	.Where(f => f.EndsWith(".sln"))
				                        	.Select(f => f));

			// mark the solution file that we are going to use as a top-level compile target:
			Solution = solution; 

			var content = File.ReadAllText(solution);

			var lines = content.Split(new string[] { System.Environment.NewLine },
			                          StringSplitOptions.RemoveEmptyEntries);

			foreach (var line in lines)
			{
				if (line.StartsWith("Project") == true)
				{
					// extract out unit test project name plus path:
					var project = line.Split(new string[] { "," },
					                         StringSplitOptions.RemoveEmptyEntries)[1].Replace(" ", "");

					// create the file name and location for the unit test found in the solution file:   
					var library_location_in_solution_file = project.Substring(1, project.Length - 2);
					var library_location_on_disk = Path.Combine(this.solution_file_path, library_location_in_solution_file);

					if (File.Exists(library_location_on_disk))
					{
						var index = new Index { IsTestProject = line.ToLower().Contains("test"), 
							ProjectFilePath = library_location_on_disk };

						// we will take the "debug" output libraries as a default
						var extracted_file_extension = string.Concat(Path.GetFileNameWithoutExtension(library_location_on_disk), ".dll");
						var extracted_file_name = string.Format(@"{0}\bin\Debug\{1}", Path.GetDirectoryName(library_location_on_disk), extracted_file_extension);
						index.ProjectLibraryPath = extracted_file_name;

						if(Indexes.ContainsKey(library_location_on_disk) == false)
						{
							if(Indexes.TryAdd(library_location_on_disk, index))
							{
								Console.WriteLine("indexing project:" + index.ProjectFilePath);
								FindAllCompileTimeItemsForIndex(index);
							}
						}
					}
				}
			}
		}

		public void Dispose()
		{
			this.Disposing(true);
			GC.SuppressFinalize(this);
		}

		private static void FindAllCompileTimeItemsForIndex(Index index)
		{
			const string compile_mask = "<Compile Include=";
			var contents = File.ReadAllText(index.ProjectFilePath);
			var lines = contents.Split(new string[] {System.Environment.NewLine}, 
				StringSplitOptions.RemoveEmptyEntries);

			foreach (var line in lines)
			{
				if(!line.Trim().StartsWith(compile_mask)) continue;

				var classPath = line.Trim().Replace(compile_mask, string.Empty);
				classPath = classPath.Replace(@"/>", string.Empty).Replace("\"", string.Empty);
				classPath = classPath.Replace(">", string.Empty);
				var classFilePath = Path.Combine(Path.GetDirectoryName(index.ProjectFilePath), classPath.Trim());
					//new FileInfo(index.ProjectFilePath).DirectoryName, classPath.Trim());
				index.ClassFiles.Add(classFilePath);
			}
		}

		private void OnIndexerTimerElapsed(object sender, ElapsedEventArgs e)
		{
			this.Index();
		}

		private void Disposing(bool disposing)
		{
			if(disposing == true)
			{
				this.disposed = true;

				if(this.indexer_timer != null)
				{
					this.indexer_timer.Stop();
					this.indexer_timer.Elapsed -= OnIndexerTimerElapsed;
				}
				this.indexer_timer = null;
			}
		}
	}
}
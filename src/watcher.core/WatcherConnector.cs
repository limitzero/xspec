using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace watcher.core
{
	public class WatcherConnector
	{
		private static ConcurrentBag<BaseWatcherExecutionPipeline> pipelines;
		private readonly Publisher publisher; 

		public WatcherConnector()
		{
			pipelines = new ConcurrentBag<BaseWatcherExecutionPipeline>();
			this.publisher = new Publisher();
		}

		public Publisher EventPublisher
		{
			get { return publisher; }
		}

		public void Configure()
		{
			find_all_pipeline_configurations_in_current_executable_directory_and_configure();
		}

		private void find_all_pipeline_configurations_in_current_executable_directory_and_configure()
		{
			var files = Directory.GetFiles(System.AppDomain.CurrentDomain.BaseDirectory, "*.dll");

			var files_to_search = files
				.Where(f => f.Contains(this.GetType().Namespace) == false)
				.ToList();

			foreach (var file in files_to_search)
			{
				Assembly assembly = null;
				Exception load_assembly_exception = null;
				if(try_load_assembly_from_file(file, out assembly, out load_assembly_exception) == false)
					continue;

				IList<Type> pipeline_executors = null;
				if(try_extract_pipeline_executors_from_assembly(assembly, out pipeline_executors) == true)
				{
					foreach (var pipeline_executor in pipeline_executors)
					{
						BaseWatcherExecutionPipeline execution_pipeline = null;
						Exception create_pipeline_executor_exception = null;
						if(try_create_pipeline_executor_from_type(pipeline_executor, assembly, 
						                                          out execution_pipeline, out create_pipeline_executor_exception) == true)
						{
							Console.WriteLine(string.Format("Connecting and configuring pipeline '{0}' for watch execution...", execution_pipeline.GetType().Name));
							execution_pipeline.Configure();
							pipelines.Add(execution_pipeline);
						}
					}
				}

			}
		}

		private bool try_load_assembly_from_file(string file, out Assembly assembly, out Exception load_assembly_exception)
		{
			bool success = false;
			load_assembly_exception = null;
			assembly = null; 

			try
			{
				assembly = Assembly.LoadFile(file);
				success = true;
			}
			catch (Exception try_load_assembly_exception)
			{
				load_assembly_exception = try_load_assembly_exception;
				throw;
			}

			return success;
		}

		private bool try_extract_pipeline_executors_from_assembly(Assembly assembly, out IList<Type> pipeline_executors)
		{
			bool success = false;
			pipeline_executors = null;

			pipeline_executors = assembly.GetTypes()
				.Where(t => t.IsClass)
				.Where(t => t.IsAbstract == false)
				.Where(t => typeof (BaseWatcherExecutionPipeline).IsAssignableFrom(t))
				.Select(t => t)
				.ToList();

			if (pipeline_executors != null && pipeline_executors.Count > 0)
				success = true;

			return success;
		}

		private bool try_create_pipeline_executor_from_type(Type executor_type, Assembly executor_assembly, out BaseWatcherExecutionPipeline executionPipeline, 
		                                                    out Exception create_pipeline_from_type_exception)
		{
			bool success = false;
			executionPipeline = null;
			create_pipeline_from_type_exception = null;

			try
			{
				executionPipeline = executor_assembly.CreateInstance(executor_type.FullName) as BaseWatcherExecutionPipeline;
				success = true;
			}
			catch (Exception try_create_pipeline_executor_from_type_exception)
			{
				create_pipeline_from_type_exception = try_create_pipeline_executor_from_type_exception;
			}
			return success;
		}

		public class Publisher
		{
			public void Publish(FileChangedEvent @event)
			{
				System.Threading.ThreadPool.QueueUserWorkItem(PublishAsync, @event);	
			}

			private void PublishAsync(object state)
			{
				var @event = state as FileChangedEvent;

				if(pipelines == null) return;

				foreach (var pipeline in pipelines)
				{
					pipeline.Execute(@event);
				}
			}
		}
	}
}
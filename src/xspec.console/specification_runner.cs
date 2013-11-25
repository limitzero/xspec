using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using xSpec;
using xSpec.Processor;

namespace xspec.console
{
	public class specification_runner
	{
		private readonly string working_directory;

		public specification_runner(string working_directory = "")
		{
			this.working_directory = working_directory;
		}

		public void run()
		{
			IEnumerable<Assembly> assemblies = this.probe_for_test_assemblies_in_working_directory();

			foreach (var test_assembly in assemblies)
			{
				ICollection<Type> specifications = null;
				if (this.try_get_specifications_from_assembly(test_assembly, out specifications) == true)
				{
					this.create_timed_run_for_test_assembly_specifications(test_assembly, specifications);
				}
			}

		}

		private IEnumerable<Assembly> probe_for_test_assemblies_in_working_directory()
		{
			const string mask = "test";

			var current_directory = string.IsNullOrEmpty(this.working_directory)
			                        	? System.Environment.CurrentDirectory
			                        	: this.working_directory;

			var files = Directory.GetFiles(current_directory, "*.dll");

			var test_library_files = files.Where(f => f.ToLower().Contains(mask)).Distinct().ToList();
			var test_assemblies = new List<Assembly>();

			foreach (var test_library in test_library_files)
			{
				Assembly test_assembly  = null;
				Exception extract_test_assembly_exception = null;

				if(try_get_assembly(test_library, out test_assembly, out extract_test_assembly_exception) == true)
				{
					test_assemblies.Add(test_assembly);
				}
			}
			return test_assemblies;
		}

		private void create_timed_run_for_test_assembly_specifications(Assembly testAssembly, ICollection<Type> specifications)
		{
			Stopwatch sw = new Stopwatch();
			List<specification_processor_result> results = new List<specification_processor_result>();

			sw.Start();

			try
			{
				foreach (var specification in specifications)
				{
					specification executable_specification = null;
					Exception create_specification_exception = null;

					if (this.try_create_specification_from_type(testAssembly, specification, out executable_specification, out create_specification_exception) == true)
					{
						specification_processor processor = new specification_processor(executable_specification);
						var specification_processor_result = processor.process();

						results.Add(specification_processor_result);
					}
				}
			}
			catch (Exception timedTestRunException)
			{
				Console.WriteLine(timedTestRunException);
			}
			finally
			{
				sw.Stop();
				this.display_test_run_results(sw, results);
			}

		}

		private void display_test_run_results(Stopwatch stopwatch, ICollection<specification_processor_result> results)
		{
			string test_run_message = "{0} examples, {1} conditions, {2} passed, {3} failed, {4} skipped, ({5} seconds) (xspec)";

			if(results.Count == 0) return;

			var accumulated_specification_processor_result = new specification_processor_result();

			new List<specification_processor_result> (results)
				.ForEach( r=>
				          	{
				          		accumulated_specification_processor_result.verbalized_specification +=
				          			string.Concat(r.verbalized_specification, System.Environment.NewLine);
				          		accumulated_specification_processor_result.number_of_examples += r.number_of_examples;
								accumulated_specification_processor_result.number_of_conditions += r.number_of_conditions;
								accumulated_specification_processor_result.number_of_passed_conditions += r.number_of_passed_conditions;
								accumulated_specification_processor_result.number_of_failed_conditions += r.number_of_failed_conditions;
								accumulated_specification_processor_result.number_of_pending_conditions += r.number_of_pending_conditions;
				          	});

			decimal seconds = ((decimal) stopwatch.Elapsed.Milliseconds/1000M);
			string failure_message = string.Empty;

			if(accumulated_specification_processor_result.number_of_failed_conditions > 0)
			{
				failure_message = this.create_failure_messages_for_test_run(new List<specification_processor_result>(results));
			}

			Console.WriteLine(string.Concat(System.Environment.NewLine,
				accumulated_specification_processor_result.verbalized_specification,
				failure_message,
				string.Format(test_run_message, 
				accumulated_specification_processor_result.number_of_examples, 
				accumulated_specification_processor_result.number_of_conditions, 
				accumulated_specification_processor_result.number_of_passed_conditions, 
				accumulated_specification_processor_result.number_of_failed_conditions, 
				accumulated_specification_processor_result.number_of_pending_conditions,
				seconds.ToString("0.####"))));
		}

		private string create_failure_messages_for_test_run(List<specification_processor_result> results)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine();
			builder.AppendLine("************ FAILURES ************");
			results.Distinct().ToList().ForEach(r => builder.AppendLine(r.failure_message));
			builder.AppendLine();
			return builder.ToString();
		}

		private bool try_create_specification_from_type(Assembly testAssembly, Type specification_type, out specification executable_specification, out Exception create_specification_exception)
		{
			bool success = false;
			executable_specification = null;
			create_specification_exception = null; 

			try
			{
				executable_specification = testAssembly.CreateInstance(specification_type.FullName) as specification;
				success = true;
			}
			catch (Exception try_create_specification_exception)
			{
				create_specification_exception = try_create_specification_exception;
			}

			return success;
		}

		private bool try_get_assembly(string assemblyName, out Assembly assembly, out Exception exception)
		{
			bool success = true;
			assembly = null;
			exception = null;

			if (assemblyName.Contains(".dll") == true && assemblyName.Contains(@"\") == false)
			{
				try
				{
					assembly = Assembly.Load(assemblyName.Replace(".dll", string.Empty));
				}
				catch (Exception loadAssemblyException)
				{
					exception = loadAssemblyException;
					success = false;
				}
			}

			if (assemblyName.Contains(@"\") &&  File.Exists(assemblyName) == true)
			{
				try
				{
					assembly = Assembly.LoadFile(assemblyName);
				}
				catch (Exception loadAssemblyException)
				{
					exception = loadAssemblyException;
					success = false;
				}
			}

			return success;
		}

		private bool try_get_specifications_from_assembly(Assembly testAssembly, out ICollection<Type> specifications)
		{
			bool success = false;
			specifications = new List<Type>();

			specifications = (from match in testAssembly.GetTypes()
							  where match.IsClass == true
									&& match.IsAbstract == false
									&& typeof(specification).IsAssignableFrom(match)
									&& typeof(specification) != match 
									&& match.Namespace.Contains(typeof(specification).Namespace) == false
							  select match).ToList();

			if (specifications != null && specifications.Count() > 0)
				success = true;

			return success;
		}

	}
}
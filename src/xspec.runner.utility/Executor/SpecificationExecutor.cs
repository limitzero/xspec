using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using xSpec;
using xspec.runner.utility.Processor;
using xspec.runner.utility.Result;
using xspec.runner.utility.Settings;
using xspec.runner.utility.Writer;

namespace xspec.runner.utility.Executor
{
	public class SpecificationExecutor : ISpecificationExecutor
	{
		private readonly ISpecificationWriter specification_writer;

		public SpecificationExecutor()
			:this(new ConsoleSpecificationWriter())
		{

		}

		public SpecificationExecutor(ISpecificationWriter specificationWriter)
		{
			specification_writer = specificationWriter;
		}

		public IEnumerable<SpecificationResult> Execute(RunSettings settings)
		{
			return ExecuteInternal(settings as dynamic);
		}

		private IEnumerable<SpecificationResult> ExecuteInternal(ClassOnlyRunSettings settings)
		{
			Type specificationType = new TypeResolver().FindSpecificationForClassName(settings.TargetAssembly, settings.TargetClass);
			return DispatchForProcessing(new Type[] { specificationType });
		}

		private IEnumerable<SpecificationResult> ExecuteInternal(ClassTypeOnlyRunSettings settings)
		{
			return DispatchForProcessing(new Type[] { settings.TargetClassType });
		}

		private IEnumerable<SpecificationResult> ExecuteInternal(AssemblyOnlyRunSettings settings)
		{
			var specificationTypes = new TypeResolver().FindAllSpecificationsInAssembly(settings.TargetAssembly);
			return DispatchForProcessing(specificationTypes);
		}

		private IEnumerable<SpecificationResult> ExecuteInternal(NamespaceOnlyRunSettings settings)
		{
			var specificationTypes = new TypeResolver().FindAllSpecificationsInNamespace(settings.TargetAssembly, settings.TargetNamespace);
			return DispatchForProcessing(specificationTypes);
		}

		private IEnumerable<SpecificationResult> ExecuteInternal(AllAssembliesInPathOnlyRunSettings settings)
		{
			var specificationTypes = new TypeResolver().FindAllSpecificationsInPath(settings.TargetPath);
			return DispatchForProcessing(specificationTypes);
		}

		private IEnumerable<SpecificationResult> ExecuteInternal(InstanceOnlyRunSettings settings)
		{
			return DispatchForProcessing(new Type[] {}, settings.TargetInstance);
		}

		private IEnumerable<SpecificationResult> DispatchForProcessing(IEnumerable<Type> specificationTypes, specification singleSpecification = null)
		{
			var results = new List<SpecificationResult>();
			var resolver = new TypeResolver();
			
			if(singleSpecification != null)
			{
				var processedResult = ProcessSpecification(singleSpecification);
				results.Add(processedResult);
			}

			foreach (var specificationType in specificationTypes)
			{
				specification createdSpecification = null;
				Exception createSpecificationException = null;

				if (resolver.TryCreateSpecificationFromType(specificationType, 
					out createdSpecification,
					out createSpecificationException) == false)
				{
					Trace.WriteLine(string.Format("Error creating specification type '{0}'. Reason {1}",
						specificationType.FullName, 
					  createSpecificationException));
					continue;
				}
			
				var processedResult = ProcessSpecification(createdSpecification);
				results.Add(processedResult);
			}

			DisplayInformationForTimedRun(results);

			return results;
		}

		private static SpecificationResult ProcessSpecification(specification currentSpecification)
		{
			var processor = new SpecificationProcessor(currentSpecification);
			var processedResult = processor.Process();
			return processedResult;
		}

		private void DisplayInformationForTimedRun(ICollection<SpecificationResult> results)
		{
			string test_run_message = "{0} examples, {1} conditions, {2} passed, {3} failed, {4} skipped, ({5} seconds) (xSpec)";

			if (results.Count == 0)
			{
				var message = "No test conditions found.";
				specification_writer.WriteLine(message);
				//Console.WriteLine(message);
				//System.Diagnostics.Debug.WriteLine(message);
				//Trace.WriteLine(message);
			}
			
			var accumulated_specification_processor_result = new SpecificationResult();

			var accumulated = new List<SpecificationResult>(results); 

			accumulated.ForEach(r =>
				{
					accumulated_specification_processor_result.verbalized_specification +=
						string.Concat(r.verbalized_specification, System.Environment.NewLine);
					accumulated_specification_processor_result.number_of_examples += r.number_of_examples;
					accumulated_specification_processor_result.number_of_conditions += r.number_of_conditions;
					accumulated_specification_processor_result.number_of_passed_conditions += r.number_of_passed_conditions;
					accumulated_specification_processor_result.number_of_failed_conditions += r.number_of_failed_conditions;
					accumulated_specification_processor_result.number_of_pending_conditions += r.number_of_pending_conditions;
				});

			decimal seconds = accumulated.Sum(a => a.duration)/100000M;
			string failure_message = string.Empty;

			if (accumulated_specification_processor_result.number_of_failed_conditions > 0)
			{
				failure_message = CreateFailureMessagesForTestRun(new List<SpecificationResult>(results));
			}

			var executor_results = string.Concat(System.Environment.NewLine,
				accumulated_specification_processor_result.verbalized_specification,
				failure_message,
				string.Format(test_run_message,
				accumulated_specification_processor_result.number_of_examples,
				accumulated_specification_processor_result.number_of_conditions,
				accumulated_specification_processor_result.number_of_passed_conditions,
				accumulated_specification_processor_result.number_of_failed_conditions,
				accumulated_specification_processor_result.number_of_pending_conditions,
				seconds.ToString("0.####")));

			this.specification_writer.WriteLine(executor_results);
			//System.Diagnostics.Debug.WriteLine(executor_results);
			//Trace.WriteLine(executor_results);
		}

		private static string CreateFailureMessagesForTestRun(List<SpecificationResult> results)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine();
			builder.AppendLine("************ FAILURES ************");

			foreach (var result in results.Distinct().ToList())
			{
				foreach (var condition in result.failure_conditions)
				{
					builder.AppendLine(condition);
				}
			}

			builder.AppendLine();
			return builder.ToString();
		}

	}
}
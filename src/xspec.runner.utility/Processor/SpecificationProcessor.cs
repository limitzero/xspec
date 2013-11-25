using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using xSpec;
using xSpec.Internal;
using xSpec.Mocking;
using xspec.runner.utility.Result;

namespace xspec.runner.utility.Processor
{
	public class SpecificationProcessor : ISpecificationProcessor
	{
		private const string single_space = " ";
		private const string single_tab = "\t";
		private const int standard_offset = 2;

		private readonly specification specification;
		private IEnumerable<Action> before_each_methods;
		private readonly SpecificationResult specification_processor_result;

		public IList<string> statements { get; set; }

		public SpecificationProcessor(specification specification)
		{
			this.specification = specification;
			this.statements = new List<string>();
			this.specification_processor_result = new SpecificationResult();
		}

		public SpecificationResult Process()
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			this.statements.Add(this.normalize_text(this.specification.GetType().Name));
			this.process_example_methods(this.find_all_example_names_with_invokable_action());

			stopwatch.Stop();
			this.specification_processor_result.duration = Convert.ToDecimal(stopwatch.ElapsedMilliseconds) * 1000M;

			this.specification_processor_result.verbalized_specification = this.flush();

			return this.specification_processor_result;
		}

		private void process_example_methods(List<KeyValuePair<string, Action>> example_methods_with_invokable_actions)
		{
			foreach (var example_method_and_invokable_action in example_methods_with_invokable_actions)
			{
				this.specification_processor_result.number_of_examples++;

				this.push_example_statement_on_stack(example_method_and_invokable_action);

				// if we are using the "observes" sub-class, instantiate the SUT first:
				this.process_subject_under_test_construction_before_example_execution();

				example_method_and_invokable_action.Value.Invoke();

				// execute the "before each" methods to load content before the execution of the scenario:
				this.before_each_methods = this.find_all_before_each_methods_for_specification();
				new List<Action>(before_each_methods).ForEach(b => b.Invoke());

				// execute the "establish" method before any common test conditions are applied:
				this.execute_current_establish_condition();
				//if (this.specification.establish != null)
				//    this.specification.establish();

				// execute the "act" method for the common action that should be applicable 
				// across all test cases for the given scenario (if specified):
				if (this.specification.act != null)
				{
					this.specification.act.Invoke();
					this.specification.act = null; // wait for next one to be defined:
				}

				// execute the test conditions in the current scope and under the "context" sub-scope:
				this.process_test_conditions_under_current_scope();
				this.process_context_conditions_under_current_scope();

				// if we are using the "observes" sub-class, clean the SUT,
				// clean-up the mocked instances within the observable, after test conditions for a clean run:
				if (typeof(IObserves).IsAssignableFrom(this.specification.GetType()) == true)
				{
					((IObserves)this.specification).clean_up();
				}
			}
		}

		private void process_subject_under_test_construction_before_example_execution()
		{
			// create the subject under test if within an observable, first before test conditions:
			if (typeof(IObserves).IsAssignableFrom(this.specification.GetType()) == true)
			{
				((IObserves)this.specification).create_subject();
			}
		}

		public void process_test_conditions_under_current_scope(int scope_level = 1)
		{
			InvokableCondition test_condition = null;
			while (this.specification.Conditions.TryDequeue(out test_condition) == true)
			{
				this.specification_processor_result.number_of_conditions++;

				var test_result = this.execute_test_condition(test_condition);
				var statement = this.tabify_message(scope_level + standard_offset, test_result);

				this.statements.Add(statement);
			}
		}

		public void process_context_conditions_under_current_scope(int current_scope_level = 0)
		{
			InvokableContextCondition context_condition = null;

			while (this.specification.Contexts.TryTake(out context_condition) == true)
			{
				// execute the "before each" methods to load content before the execution of the scenario:
				this.before_each_methods = this.find_all_before_each_methods_for_specification();
				new List<Action>(before_each_methods).ForEach(b => b.Invoke());

				// configure any sub-context:
				if(context_condition.Configure != null)
					context_condition.Configure();

				var statement = this.tabify_message(current_scope_level + standard_offset, string.Format("{0}", context_condition.Context));
				this.statements.Add(statement);

				// execute the "act" method for the common action that should be applicable 
				// across all test cases for the given scenario (if specified):
				if (this.specification.act != null)
				{
					this.specification.act.Invoke();
					this.specification.act = null; // wait for next one to be defined:
				}

				// execute the "establish" method before any test conditions are examined:
				//if (this.specification.establish != null)
				//    this.specification.establish();

				this.process_test_conditions_under_current_scope(current_scope_level + 1);

				if (this.specification.Contexts != null && this.specification.Contexts.Count > 0)
				{
					this.process_context_conditions_under_current_scope(current_scope_level + 1);
				}
			}
		}

		private string execute_test_condition(InvokableCondition test_condition)
		{
			string result = test_condition.ToString();

			// execute the "establish" method before any test conditions are examined:
			this.execute_current_establish_condition();
			//if (this.specification.establish != null)
			//{
			//    this.specification.establish();
			//}

			if (test_condition.IsPending == false)
			{
				if (test_condition.Configure == null) return result;

				try
				{
					test_condition.Configure.Invoke();
					this.specification_processor_result.number_of_passed_conditions++;
				}
				catch (Exception test_condition_exception)
				{
					result = write_failing_condition(test_condition, test_condition_exception);
					this.specification_processor_result.number_of_failed_conditions++;
					this.specification_processor_result.create_failure_message(string.Format("{0}\r{1}",
																							 result, test_condition_exception.ToString()));
				}
			}
			else
			{
				this.specification_processor_result.number_of_pending_conditions++;
			}

			return result;
		}

		private void execute_current_establish_condition()
		{
			InvokableCondition establishCondition = null;
			if (this.specification.EstablishConditions.TryDequeue(out establishCondition))
				establishCondition.Configure();
		}

		private static string write_failing_condition(InvokableCondition condition, Exception exception)
		{
			return string.Format("{0} - FAILED - {1}", condition, exception.Message);
		}

		private IEnumerable<Action> find_all_before_each_methods_for_specification()
		{
			IList<Action> before_each_methods = new List<Action>();

			var flags = BindingFlags.Instance | BindingFlags.NonPublic;

			var methods = this.specification.GetType().GetMethods(flags)
				.Where(m => m.Name.ToLower().StartsWith("before_each_"))
				.Select(m => m).ToList();

			methods.ForEach(m => before_each_methods.Add(() => m.Invoke(this.specification, null)));

			if (this.specification.before_each != null)
			{
				before_each_methods.Add(this.specification.before_each);
			}

			return before_each_methods;
		}

		private void push_example_statement_on_stack(KeyValuePair<string, Action> example_method_and_invokable_action)
		{
			this.statements.Add(this.tabify_message(standard_offset, example_method_and_invokable_action.Key));
		}

		public string flush()
		{
			StringBuilder builder = new StringBuilder();
			new List<string>(this.statements).ForEach(s => builder.AppendLine(s));

			var result = builder.ToString();
			//Trace.Write(result);

			return result;
		}

		private List<KeyValuePair<string, Action>> find_all_example_names_with_invokable_action()
		{
			var flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
			var names = new List<string> { "specify", "when", "given" };

			var example_name_and_invokable_method = (from match in this.specification.GetType().GetMethods(flags)
													 from name in names
													 let kvp =
														new KeyValuePair<string, Action>(this.normalize_text(match.Name),
																						 () => { match.Invoke(this.specification, null); })
													 where match.Name.Trim().ToLower().StartsWith(name)
													 select kvp).ToList();

			return example_name_and_invokable_method;
		}

		private string normalize_text(string text)
		{
			return text.Replace("_", single_space);
		}

		private string tabify_message(int levels, string message)
		{
			string tab = single_space;

			for (int index = 1; index < levels; index++)
			{
				tab += tab;
			}

			return string.Concat(tab, this.normalize_text(message));
		}
	}
}
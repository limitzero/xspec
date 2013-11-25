using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using xSpec.Internal;
using xSpec.Processor;

namespace xSpec
{
	/// <summary>
	/// Basic construct for starting to define test conditions and scenarios for 
	/// identifying the behavior of a subject under test. 
	/// </summary>
	public abstract class specification
	{
		private readonly specification_processor specification_processor;
		private ConcurrentQueue<InvokableCondition> test_conditions = new ConcurrentQueue<InvokableCondition>();
		private ConcurrentQueue<InvokableCondition> establish_conditions = new ConcurrentQueue<InvokableCondition>();
		private ConcurrentBag<InvokableContextCondition> test_contexts = new ConcurrentBag<InvokableContextCondition>();

		/// <summary>
		/// Place holder for a pending test condition
		/// </summary>
		protected readonly Action todo = () => { };

		/// <summary>
		/// Gets the collection of individual test conditions for the specification
		/// </summary>
		public ConcurrentQueue<InvokableCondition> Conditions { get { return test_conditions; } }

		/// <summary>
		/// Gets the collection of individual test conditions for the specification
		/// </summary>
		public ConcurrentQueue<InvokableCondition> EstablishConditions { get { return establish_conditions; } }

		/// <summary>
		/// Gets the colllection of test contexts that specification sub-scenarions for a given specification:
		/// </summary>
		public ConcurrentBag<InvokableContextCondition> Contexts
		{ 
			get { return test_contexts; }
			private set { test_contexts = value; }
		}

		/// <summary>
		/// Action that is run before every execution of the specification.
		/// </summary>
		public Action before_each { get; set; }

		private Action establishField;
		/// <summary>
		/// The pre-condition action runs first to set-up subject under test for the subsequent test conditions.
		/// </summary>
		public Action establish
		{
			get { return establishField; }
			set
			{
				establishField = value;
				var establish_condition = new InvokableCondition(establishField);
				this.establish_conditions.Enqueue(establish_condition);
			}
		}

		/// <summary>
		/// The single or set of actions that will be enacted once over the specification setup for multiple test conditions.
		/// </summary>
		public Action act { get; set; }

		/// <summary>
		/// The "test condition" that is to be inspected or expected on the subject under test.
		/// </summary>
		public InvokableCondition it
		{
			get
			{
				var test_condition = new InvokableCondition();
				this.test_conditions.Enqueue(test_condition);
				return test_condition;
			}
		}

		/// <summary>
		/// The pending test condition that will later be inspected or expected on the subject under test.
		/// </summary>
		public InvokableCondition xit
		{
			get
			{
				var test_condition = new InvokableCondition(true);
				this.test_conditions.Enqueue(test_condition);
				return test_condition;
			}
		}

		/// <summary>
		/// The area for a subject under test where the testing conditions can be expanding for multiple scenarios.
		/// Nesting of contexts is not supported.
		/// </summary>
		public InvokableContextCondition context
		{
			get
			{
				var test_context = new InvokableContextCondition();
				this.test_contexts.Add(test_context);
				return test_context;
			}
		}

		/// <summary>
		/// This will excecute an action and return the indicated exception, if generated
		/// </summary>
		/// <param name="action">The action to execute within the test condition</param>
		/// <returns>
		/// NoExceptionWasThrownButWasExpectedException : if the action does not throw an exception
		/// </returns>
		protected Exception catch_exception(Action action)
		{
			Exception theCaughtException = new NoExceptionWasThrownButWasExpectedException();
			try
			{
				action.Invoke();
			}
			catch (Exception exception)
			{
				theCaughtException = exception;
			}
	
			return theCaughtException;
		}

		/// <summary>
		/// This will excecute an action and return the indicated exception, if generated
		/// </summary>
		/// <param name="action">The action to execute within the test condition</param>
		/// <typeparam name="T">The type of exception that should be returned</typeparam>
		/// <returns>
		/// NoExceptionWasThrownButWasExpectedException : if the action does not throw an exception
		/// </returns>
		protected T catch_exception<T>(Action action) where T : Exception
		{
			Exception theCaughtException = catch_exception(action);
			return theCaughtException as T;
		}

		protected specification()
		{
			this.specification_processor = new specification_processor(this);
		}

		public void execute()
		{
			this.test_contexts = new ConcurrentBag<InvokableContextCondition>(this.test_contexts.Reverse());
			this.specification_processor.process();
			this.specification_processor.flush();
		}
	}
}
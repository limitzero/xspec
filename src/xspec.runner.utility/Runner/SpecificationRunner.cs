using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using xSpec;
using xspec.runner.utility.Executor;
using xspec.runner.utility.Result;
using xspec.runner.utility.Settings;
using xspec.runner.utility.Writer;

namespace xspec.runner.utility.Runner
{
	public class SpecificationRunner : ISpecificationRunner
	{
		private readonly ISpecificationExecutor executor;

		public ISpecificationWriter Writer { get; private set; }

		public event Action<SpecificationResultException> OnRunnerFailure;

		public SpecificationRunner()
			: this(new ConsoleSpecificationWriter())
		{

		}

		public SpecificationRunner(ISpecificationWriter specificationWriter)
		{
			this.Writer = specificationWriter;
			this.executor = new SpecificationExecutor(this.Writer);
		}
		
		public void RunAllInPath(string targetPath)
		{
			var settings = new AllAssembliesInPathOnlyRunSettings(targetPath);
			var results = this.executor.Execute(settings);
			this.NotifyOnFailures(results);
		}

		public void RunAssembly(Assembly assembly)
		{
			var settings = new AssemblyOnlyRunSettings(assembly);
			var results = this.executor.Execute(settings);
			this.NotifyOnFailures(results);
		}

		public void RunNamespace(Assembly assembly, string targetNamespace)
		{
			var settings = new NamespaceOnlyRunSettings(assembly, targetNamespace);
			var results = this.executor.Execute(settings);
			this.NotifyOnFailures(results);
		}

		public void RunClass(Assembly assembly, string targetClassName)
		{
			var settings = new ClassOnlyRunSettings(assembly, targetClassName);
			var results = this.executor.Execute(settings);
			this.NotifyOnFailures(results);
		}

		public void RunType(Type classType)
		{
			if(typeof(specification).IsAssignableFrom(classType) == false)
				return;

			var settings = new ClassTypeOnlyRunSettings(classType);
			var results = this.executor.Execute(settings);
			this.NotifyOnFailures(results);
		}

		public void RunInstance(specification instance)
		{
			var settings = new InstanceOnlyRunSettings(instance);
			var results = this.executor.Execute(settings);
			this.NotifyOnFailures(results);
		}

		private void NotifyOnFailures(IEnumerable<SpecificationResult> results)
		{
			var evt = this.OnRunnerFailure;
			var error = new SpecificationResultException(results);

			if (evt != null && error.FailureConditions.Count() > 0)
			{
				evt(error);
			}
		}

	}
}
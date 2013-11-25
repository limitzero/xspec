using System;
using System.Reflection;
using xSpec;
using xspec.runner.utility.Result;
using xspec.runner.utility.Settings;
using xspec.runner.utility.Writer;

namespace xspec.runner.utility.Runner
{
	/// <summary>
	/// Represents a high-level test runner
	/// </summary>
	public interface ISpecificationRunner
	{
		ISpecificationWriter Writer { get; }

		/// <summary>
		/// Event that is triggered to caller when a specification run has an exception.
		/// </summary>
		event Action<SpecificationResultException> OnRunnerFailure;

		/// <summary>
		/// Executes all tests in tests assemblies found in the specified path.
		/// </summary>
		/// <param name="targetPath"></param>
		void RunAllInPath(string targetPath);

		/// <summary>
		/// Executes the specifications inside of the assembly
		/// </summary>
		/// <returns></returns>
		void RunAssembly(Assembly assembly);

		/// <summary>
		/// Executes the specifications inside of the assembly under a specific namespace
		/// </summary>
		/// <returns></returns>
		void RunNamespace(Assembly aseembly, string assemblyNamespace);

		/// <summary>
		/// Runs the class that represents a specification.
		/// </summary>
		/// <param name="assembly">The assembly holding the class</param>
		/// <param name="className">The type name of the class</param>
		/// <returns></returns>
		void RunClass(Assembly assembly, string className);

		/// <summary>
		/// Runs the tests in the class that is assignable from the specification base class.
		/// </summary>
		/// <param name="classType"></param>
		void RunType(Type classType);

		/// <summary>
		/// Runs the current instance of the specification.
		/// </summary>
		/// <param name="instance"></param>
		void RunInstance(specification instance);

		
	}
}
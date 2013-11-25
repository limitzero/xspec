using System;
using System.Reflection;
using xSpec;

namespace xspec.runner.utility.Settings
{
	/// <summary>
	///  Base class to provide the run settings for a particular configuration.
	/// </summary>
	[Serializable]
	public abstract class RunSettings
	{
		/// <summary>
		/// Gets the target assembly to search for running tests. 
		/// </summary>
		public Assembly TargetAssembly { get; private set; }

		/// <summary>
		/// Gets the target namespace in an assembly for running tests.
		/// </summary>
		public string TargetNamespace { get; private set; }

		/// <summary>
		/// Gets the target class in an assembly for running tests.
		/// </summary>
		public string TargetClass { get; private set; }

		/// <summary>
		/// Gets the target path where test assemblies will be located to run tests.
		/// </summary>
		public string TargetPath { get; private set; }

		/// <summary>
		/// Gets the target class type of the specification to execute:
		/// </summary>
		public Type TargetClassType { get; private set; }

		/// <summary>
		/// Gets the current specification instance to execute.
		/// </summary>
		public specification TargetInstance { get; private set; }

		protected RunSettings(Assembly targetAssembly, string targetNamespace, 
			string targetClass, string targePath, 
			Type targetClassType, specification targetInstance)
		{
			this.TargetAssembly = targetAssembly;
			this.TargetNamespace = targetNamespace;
			this.TargetClass = targetClass;
			this.TargetPath = targePath;
			this.TargetClassType = targetClassType;
			this.TargetInstance = targetInstance;
		}
	}
}
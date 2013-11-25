using System;
using System.Reflection;

namespace xspec.runner.utility.Settings
{
	[Serializable]
	public class AssemblyOnlyRunSettings : RunSettings
	{
		public AssemblyOnlyRunSettings(Assembly targetAssembly)
			:base(targetAssembly, string.Empty, string.Empty,
			string.Empty, null, null)
		{
			if(targetAssembly == null)
				throw new ArgumentNullException("targetAssembly",
					"The target assembly can not be null for a test session.");
		}
	}
}
using System;
using System.Reflection;

namespace xspec.runner.utility.Settings
{
	[Serializable]
	public class ClassOnlyRunSettings : RunSettings
	{
		public ClassOnlyRunSettings(Assembly targetAssembly, string targetClassName)
			: base(targetAssembly, string.Empty, targetClassName, 
			string.Empty, null, null)
		{
			if (targetAssembly == null)
				throw new ArgumentNullException("targetAssembly", "The target assembly can not be null for a test session.");

			if (string.IsNullOrEmpty(targetClassName) == true)
				throw new ArgumentNullException("targetClassName", "The target class name can not be null for a test session.");

		}
	}
}
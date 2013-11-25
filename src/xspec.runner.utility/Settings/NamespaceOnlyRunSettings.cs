using System;
using System.Reflection;

namespace xspec.runner.utility.Settings
{
	[Serializable]
	public class NamespaceOnlyRunSettings : RunSettings
	{
		public NamespaceOnlyRunSettings(Assembly targetAssembly, string targetNamespace)
			: base(targetAssembly, targetNamespace, string.Empty, 
			string.Empty, null, null)
		{
			if (targetAssembly == null)
				throw new ArgumentNullException("targetAssembly", "The target assembly can not be null for a test session.");

			if (string.IsNullOrEmpty(targetNamespace) == true)
				throw new ArgumentNullException("targetNamespace", "The target namespace can not be null for a test session.");

		}
	}
}
using System;
using System.Reflection;

namespace xspec.runner.utility.Settings
{
	[Serializable]
	public class AllAssembliesInPathOnlyRunSettings : RunSettings
	{
		public AllAssembliesInPathOnlyRunSettings(string targetPath) : 
			base(null, string.Empty, string.Empty, 
			string.IsNullOrEmpty(targetPath) ? "."  : targetPath, null, null)
		{
		}
	}
}
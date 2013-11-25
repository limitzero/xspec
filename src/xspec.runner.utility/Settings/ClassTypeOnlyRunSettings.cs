using System;

namespace xspec.runner.utility.Settings
{
	[Serializable]
	public class ClassTypeOnlyRunSettings : RunSettings
	{
		public ClassTypeOnlyRunSettings(Type classType) 
			: base(null, string.Empty, string.Empty, string.Empty, classType, null)
		{
			
		}
	}
}
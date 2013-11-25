using System;
using xSpec;

namespace xspec.runner.utility.Settings
{
	[Serializable]
	public class InstanceOnlyRunSettings : RunSettings
	{
		public InstanceOnlyRunSettings(specification instance)
			: base(null, string.Empty, string.Empty, string.Empty, null, instance)
		{
			if(instance == null)
				throw new ArgumentNullException("instance", "The specification instance can not be null");
		}
	}
}
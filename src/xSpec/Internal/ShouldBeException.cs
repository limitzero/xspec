using System;

namespace xSpec.Internal
{
	public class ShouldBeException : ApplicationException
	{
		public ShouldBeException(string message)
			: base(message)
		{
		}
	}
}
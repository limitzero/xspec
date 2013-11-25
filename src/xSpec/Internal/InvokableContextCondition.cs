using System;

namespace xSpec.Internal
{
	public class InvokableContextCondition : specification
	{
		public string Context { get; private set; }
		public Action Configure { get; private set; }
		public Action this[string context]
		{
			set
			{
				this.Context = context;
				Configure = value;
			}
		}
	}
}
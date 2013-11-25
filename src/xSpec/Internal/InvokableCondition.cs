using System;
using System.Text;

namespace xSpec.Internal
{
	public class InvokableCondition
	{
		public bool IsPending { get; private set; }
		public string Context { get; private set; }
		public Action Configure { get; private set; }

		public InvokableCondition(Action configure)
			:this(false)
		{
			this.Configure = configure;
		}

		public InvokableCondition(bool isPending = false)
		{
			IsPending = isPending;
		}

		public Action this[string context]
		{
			set
			{
				this.Context = context;
				Configure = value;
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			if (this.IsPending == true)
			{
				sb.AppendFormat("it {0} - {1}", this.Context, "PENDING");
			}
			else
			{
				sb.AppendFormat("it {0}", this.Context);
			}

			return sb.ToString();
		}

		public override int GetHashCode()
		{
			return this.Context.GetHashCode();
		}
	}
}
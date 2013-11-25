using xspec.console.Runner;

namespace xspec.console.Modes
{
	public interface IRunMode
	{
		void Execute(ExecutingOptions executingOptions);
	}
}
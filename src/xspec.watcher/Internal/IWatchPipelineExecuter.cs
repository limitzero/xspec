namespace xSpec.watcher.Internal
{
	public interface IWatchPipelineExecuter
	{
		void Execute(string current_directory, string current_file);
	}
}
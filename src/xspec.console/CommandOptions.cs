namespace xspec.console
{
	public enum CommandOptions
	{
		/// <summary>
		/// The file name that contains the tests to run.
		/// </summary>
		Library,

		/// <summary>
		/// The runner will look in the current path for the test to run by name
		/// </summary>
		Path,

		/// <summary>
		/// Option to allow the runner to provide visual feedback on the tests (like Growl)
		/// </summary>
		FeedBack
	}
}
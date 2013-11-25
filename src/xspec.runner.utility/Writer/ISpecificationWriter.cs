namespace xspec.runner.utility.Writer
{
	public interface ISpecificationWriter
	{
		string ReadAll();
		void WriteLine(string message);
	}
}
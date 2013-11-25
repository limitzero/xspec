using System;

namespace xspec.runner.utility.Hosting
{
	public interface IExecutorHost : IDisposable
	{
		void Execute(string assemblyFilePath, string assemblyFileName);		
		void Stop();
	}
}
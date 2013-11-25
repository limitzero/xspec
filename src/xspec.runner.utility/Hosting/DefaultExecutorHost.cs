using System;
using System.Reflection;
using System.IO;
using xspec.runner.utility.Executor;
using xspec.runner.utility.Settings;

namespace xspec.runner.utility.Hosting
{
	/// <summary>
	/// Simple host for excuting the specifications.
	/// </summary>
	public class DefaultExecutorHost : MarshalByRefObject, IExecutorHost
	{
		private bool disposed;

		private readonly ISpecificationExecutor executor;

		public DefaultExecutorHost()
		{
			this.executor = new SpecificationExecutor();
		}
	
		public void Execute(string assemblyFilePath, string assemblyFileName)
		{
			var assembly = Assembly.Load(Path.GetFileNameWithoutExtension(assemblyFileName));
				//Assembly.LoadFrom(Path.Combine(assemblyFilePath, assemblyFileName));
		
			var settings = new AssemblyOnlyRunSettings(assembly);
			this.executor.Execute(settings);
		}

		public void Stop()
		{
			this.Dispose();
		}

		public void Dispose()
		{
			this.Disposing(true);
			GC.SuppressFinalize(this);
		}

		private void Disposing(bool disposing)
		{
			if (disposing == true)
			{
				this.disposed = true;
			}
		}

		private void GuardOnDispose()
		{
			if (this.disposed == true)
				throw new ObjectDisposedException("Can not access a disposed object.");
		}
	}
}
using System;

namespace xspec.runner.utility.Hosting
{
	public class HostedExecutor : IHostedExecutor
	{
		private readonly IExecutorHost host;
		private readonly string assembly_file_path;
		private readonly string assembly_file_name;
		private readonly AppDomain app_domain;

		public HostedExecutor(IExecutorHost host, 
			string assemblyFilePath, 
			string assemblyFileName,
			AppDomain appDomain)
		{
			this.host = host;
			assembly_file_path = assemblyFilePath;
			assembly_file_name = assemblyFileName;
			this.app_domain = appDomain;
		}

		public void Execute()
		{
			host.Execute(this.assembly_file_path, this.assembly_file_name);

			try
			{
				AppDomain.Unload(this.app_domain);
			}
			catch (AppDomainUnloadedException appDomainUnloadedException)
			{
				Console.WriteLine("Could not unload the app domain, most likely there is a thread that could not be aborted. Reason: " +
				                  appDomainUnloadedException);
			}
		}
	}
}
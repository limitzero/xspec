using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace xspec.runner.utility.Hosting
{
	[Serializable]
	public class RemoteExecutorHost : IDisposable
	{
		private readonly string assembly_file_path;
		private readonly string assembly_file_name;
		private bool disposed;

		private readonly string hostAsm = typeof(DefaultExecutorHost).Assembly.Location;
		private readonly string hostType = typeof(DefaultExecutorHost).FullName;
		private IHostedExecutor executor;

		public RemoteExecutorHost(string assemblyFilePath, string assemblyFileName)
		{
			this.assembly_file_path = assemblyFilePath;
			this.assembly_file_name = assemblyFileName;
		}

		public void Execute()
		{
			this.executor = CreateApplicationDomain();

			try
			{
				this.executor.Execute();
			}
			catch (ReflectionTypeLoadException e)
			{
				var sb = new StringBuilder();
				foreach (Exception exception in e.LoaderExceptions)
				{
					sb.AppendLine(exception.ToString());
				}
				throw new TypeLoadException(sb.ToString(), e);
			}
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

		private IHostedExecutor CreateApplicationDomain()
		{
			var domainName = Path.GetFileNameWithoutExtension(this.assembly_file_name);

			var setup = new AppDomainSetup
									{
										ApplicationBase = this.assembly_file_path,
										ApplicationName = this.assembly_file_name,
										ConfigurationFile = CreateConfigurationFile(),
										ShadowCopyFiles = "true"
									};

			AppDomain appDomain = AppDomain.CreateDomain(domainName, null,
				setup, new PermissionSet(PermissionState.Unrestricted));

			appDomain.AssemblyResolve += ResolveAssemblies;
			 
			return CreateRemoteHost(appDomain);
		}

		private Assembly ResolveAssemblies(object sender, ResolveEventArgs args)
		{
			Assembly assembly = null;
			bool foundAssembly = false;

#if DEBUG
			//System.Console.WriteLine("Received request for the following dll {0}", args.Name);
#endif

			int idx = args.Name.IndexOf(',');

			if (idx > 0)
			{
				string partialName = args.Name.Substring(0, idx);
				string dllName = partialName + ".dll";
				string exeName = partialName + ".exe";

				string searchDirectory = this.assembly_file_path;
				// Add other directories that you want to search here

				List<string> directorySearch = new List<string>
					{
					  Path.Combine(searchDirectory, dllName),
					  Path.Combine(searchDirectory, exeName),
					  typeof(string).Assembly.Location
					  // Include the other directories here to this list adding both the dll and exe.
					};

				foreach (string fileName in directorySearch)
				{
					if (File.Exists(fileName))
					{
						// System.Console.WriteLine("Found dll {0} at {1}", args.Name, fileName);
						foundAssembly = true;
						assembly = Assembly.LoadFrom(fileName);
						break;
					}
				}

				if (assembly == null)
				{
					if (!foundAssembly)
					{
						foreach (string fileName in directorySearch)
						{
							System.Console.WriteLine("Could not find dll {0} in any search path used {1}", args.Name, fileName);
						}
					}
					else
					{
						System.Console.WriteLine("Could not load dll {0}", args.Name);
					}
				}
			}

			return assembly;
		}

		private IHostedExecutor CreateRemoteHost(AppDomain appDomain)
		{
			object instance = appDomain.CreateInstanceFromAndUnwrap(hostAsm, hostType, null);

			var host = (IExecutorHost)instance;
			return new HostedExecutor(host, this.assembly_file_path, this.assembly_file_name, appDomain);
		}

		private string CreateConfigurationFile()
		{
			var configurationFile = string.Empty;

			var fileName = Path.GetFileNameWithoutExtension(this.assembly_file_name);

			configurationFile = Path.Combine(this.assembly_file_path, fileName + ".exe.config");

			if (File.Exists(configurationFile) == false)
				configurationFile = Path.Combine(this.assembly_file_path, fileName + ".dll.config");

			return configurationFile;
		}

		private void Disposing(bool disposing)
		{
			if (disposing == true)
			{
				this.executor = null;
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
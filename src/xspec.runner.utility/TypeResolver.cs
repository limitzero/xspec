using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using xSpec;
using xspec.runner.utility.Settings;

namespace xspec.runner.utility
{
	public class TypeResolver
	{
		public bool TryCreateSpecificationFromType(Type specificationType, 
			out specification createdSpecification, 
			out Exception createSpecificationFromTypeException)
		{
			bool success = false;
			createdSpecification = null;
			createSpecificationFromTypeException = null;
			
			try
			{
				createdSpecification =
					specificationType.Assembly.CreateInstance(specificationType.FullName) as specification;
					//Activator.CreateInstance(specificationType) as specification;
					
				if(createdSpecification != null)
					success = true;
			}
			catch (Exception tryCreateSpecificationFromTypeException)
			{
				createSpecificationFromTypeException = tryCreateSpecificationFromTypeException;
			}

			return success;
		}

		public IEnumerable<Type> FindAllSpecificationsInPath(string targetPath)
		{
			var specificationTypes = new List<Type>(); 

			// take the assumption that all tests will be written in a library and named %Test%... :)
			var files = Directory.GetFiles(targetPath, "*Test*.dll");

			foreach (var file in files)
			{
				Assembly assembly = null;
				Exception loadAssemblyException = null;

				if(TryLoadAssemblyFromFile(file, out assembly, out loadAssemblyException) == false)
					continue;

				specificationTypes.AddRange(this.FindAllSpecificationsInAssembly(assembly));
			}

			return specificationTypes;
		}

		public IEnumerable<Type> FindAllSpecificationsInAssembly(Assembly assembly)
		{
			// resolve all assemblies that can not be done via current assembly via LINQ at runtime !!!
			AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
			                                           	{
			                                           		foreach (var appDomainAsembly in ((AppDomain) sender).GetAssemblies())
			                                           		{
			                                           			if (appDomainAsembly.FullName == args.Name)
			                                           			{
			                                           				return appDomainAsembly;
			                                           			}
			                                           		}
															return null;
			                                           	};

			 	var types = assembly.GetExportedTypes()
					.Where(t => t.IsClass == true)
					.Where(t => t.IsAbstract == false)
					.Where(t => typeof(specification).IsAssignableFrom(t))
					.Select(t => t)
					.ToList();

			return types;
		}

		public IEnumerable<Type> FindAllSpecificationsInNamespace(Assembly assembly, string targetNamespace)
		{
			var targetedTestsInAssemblyByNamespace = new List<Type>(); 

			var assemblies = this.FindAllSpecificationsInAssembly(assembly);

			if(assemblies != null)
			{
				targetedTestsInAssemblyByNamespace = assemblies
					.Where(t => t.Namespace.StartsWith(targetNamespace))
					.Select(t => t)
					.ToList();
			}

			return targetedTestsInAssemblyByNamespace;
		}

		public Type FindSpecificationForClassName(Assembly assembly, string targetClassName)
		{
			Type specificationType = null; 

			var specificationTypes = this.FindAllSpecificationsInAssembly(assembly); 
			
			if(specificationTypes != null)
			{
				specificationType = specificationTypes.Where(t => t.Name.Equals(targetClassName))
					.FirstOrDefault();
			}

			return specificationType;
		}

		public bool TryLoadAssemblyFromFile(string assemblyFileName, out Assembly assembly, out Exception loadAssemblyFileException)
		{
			bool success = false;
			assembly = null;
			loadAssemblyFileException = null; 

			try
			{
				assembly = Assembly.LoadFrom(assemblyFileName);
				success = true;
			}
			catch (Exception tryLoadAssemblyFileLoadException)
			{
				loadAssemblyFileException = tryLoadAssemblyFileLoadException;
			}

			return success;
		}

		public string GetAssemblyFilePath(RunSettings settings)
		{
			Assembly assembly = FindAssembly(settings as dynamic);
			return Path.GetDirectoryName(assembly.Location);
		}

		private Assembly FindAssembly(ClassOnlyRunSettings settings)
		{
			var type = this.FindSpecificationForClassName(settings.TargetAssembly, settings.TargetClass);
			return type.Assembly;
		}

		private Assembly FindAssembly(ClassTypeOnlyRunSettings settings)
		{
			return settings.TargetClassType.Assembly;
		}

		private Assembly FindAssembly(AssemblyOnlyRunSettings settings)
		{
			return settings.TargetAssembly;
		}

		private Assembly FindAssembly(InstanceOnlyRunSettings settings)
		{
			return settings.TargetInstance.GetType().Assembly;
		}
	}
}
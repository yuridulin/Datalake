using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace iNOPC.Server.Models
{
	public class AssemblyLoader
	{
		private static readonly IDictionary<string, Assembly> Additional = new Dictionary<string, Assembly>();

		static string Path =  AppDomain.CurrentDomain.BaseDirectory + "Drivers";

		public static void Load()
		{
			using (var watcher = new FileSystemWatcher(Path))
			{
				watcher.NotifyFilter = NotifyFilters.Attributes
					| NotifyFilters.CreationTime
					| NotifyFilters.DirectoryName
					| NotifyFilters.FileName
					| NotifyFilters.Size;

				watcher.Changed += (s, e) => Reload();

				watcher.Filter = "*.dll";
				watcher.IncludeSubdirectories = true;
				watcher.EnableRaisingEvents = true;

				Console.ReadLine();
			}
		}

		private static void Reload()
		{
			var assemblies = Directory.GetFiles(Path, "*.dll");

			foreach (var assemblyName in assemblies)
			{
				try
				{
					Program.Log("Assembly Loader load from " + assemblyName);
					var file = File.ReadAllBytes(assemblyName);
					var assembly = Assembly.Load(file);

					Additional[assembly.FullName] = assembly;
				}
				catch (Exception e)
				{
					Program.Log("Assembly Loader error: " + e.Message);
				}
			}
		}

		public static void Setup()
		{
			Reload();
			AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ResolveAssembly;
			AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
		}

		private static Assembly ResolveAssembly(object sender, ResolveEventArgs e)
		{
			Additional.TryGetValue(e.Name, out Assembly res);
			return res;
		}
	}
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGetCheck.MSBuildInterop;

namespace NuGetCheck
{
	public class PackageVersionMismatchCommand : ICommand
	{
		private readonly SolutionParser _solutionParser;
		private readonly PackagesConfigParser _packagesConfigParser;

		public PackageVersionMismatchCommand(SolutionParser solutionParser, PackagesConfigParser packagesConfigParser)
		{
			_solutionParser = solutionParser;
			_packagesConfigParser = packagesConfigParser;
		}

		public int Run(string[] args, TextWriter log)
		{
			if (args.Length != 1)
			{
				throw new ApplicationException("You must specify the path of the solution to check");
			}
			var solutionPath = args[0];
			log.WriteLine("Checking solution {0}", solutionPath);

			var allPackages = new ConcurrentDictionary<string, ConcurrentDictionary<string, List<ProjectInSolution>>>();
			
			using (var streamReader = new StreamReader(File.OpenRead(solutionPath)))
			{
				_solutionParser.SolutionReader = streamReader;
				_solutionParser.ParseSolution();
			}
			var projects = _solutionParser.Projects.Where(x => !x.IsSolutionFolder).ToArray();
			log.WriteLine("Checking {0} projects", projects.Length);
			foreach (var project in projects)
			{
				var projectPath = Path.Combine(Path.GetDirectoryName(solutionPath), project.RelativePath);
				string packagesConfigPath = Path.Combine(Path.GetDirectoryName(projectPath), "packages.config");
				if (File.Exists(packagesConfigPath))
				{
					foreach (var package in _packagesConfigParser.Parse(packagesConfigPath))
					{
						allPackages.GetOrAdd(package.Id, x => new ConcurrentDictionary<string, List<ProjectInSolution>>())
							.GetOrAdd(package.Version, x => new List<ProjectInSolution>())
							.Add(project);
					}
				}
			}

			var foundConflicts = false;
			foreach (var package in allPackages.Where(x => x.Value.Count > 1))
			{
				foundConflicts = true;
				foreach (var version in package.Value)
				{
					var projectNames = string.Join(", ", version.Value.Select(x => x.ProjectName));
					log.WriteLine("Package {0}.{1} is referenced in {2}", package.Key, version.Key, projectNames);
				}
			}

			if (foundConflicts)
			{
				log.WriteLine("Solution contain conflicting packages, see previous messages.");
				return 1;
			}
			log.WriteLine("No version conflict found.");
			return 0;
		}
	}
}
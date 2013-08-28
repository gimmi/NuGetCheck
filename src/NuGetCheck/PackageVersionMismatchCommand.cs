using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NuGetCheck
{
	public class PackageVersionMismatchCommand : ICommand
	{
		private readonly SolutionParser _solutionParser;

		public PackageVersionMismatchCommand(SolutionParser solutionParser)
		{
			_solutionParser = solutionParser;
		}

		public int Run(string[] args, TextWriter log)
		{
			if (args.Length != 1)
			{
				throw new ApplicationException("You must specify the path of the solution to check");
			}

			var allPackages = new ConcurrentDictionary<string, ConcurrentDictionary<string, List<string>>>();
			List<string> projectFileNames = _solutionParser.GetAllProjectFileNames(args[0]);
			foreach (string projectFileName in projectFileNames)
			{
				string packagesConfigPath = Path.Combine(Path.GetDirectoryName(projectFileName), "packages.config");
				if (!File.Exists(packagesConfigPath))
				{
					continue;
				}
				foreach (XElement packageEl in XDocument.Load(packagesConfigPath).Element("packages").Descendants("package"))
				{
					string packageId = packageEl.Attribute("id").Value;
					string packageVerison = packageEl.Attribute("version").Value;
					log.WriteLine(string.Concat(projectFileName, ": found package '", packageId, "' version ", packageVerison));
					allPackages.GetOrAdd(packageId, x => new ConcurrentDictionary<string, List<string>>())
						.GetOrAdd(packageVerison, x => new List<string>())
						.Add(projectFileName);
				}
			}
			var multipleVersionPackages = allPackages.Where(x => x.Value.Count > 1).ToList();
			if (multipleVersionPackages.Any())
			{
				log.WriteLine("Found the following mismatch:");
				foreach (var package in multipleVersionPackages)
				{
					log.WriteLine(string.Concat("Package '", package.Key, "'"));
					foreach (var version in package.Value)
					{
						log.WriteLine(string.Concat("Version '", version.Key, "' referenced in ", string.Join(", ", version.Value.Select(x => string.Concat("'", x, "'")))));
					}
				}
				return 1;
			}

			log.WriteLine("Everything seems ok.");
			return 0;
		}
	}
}
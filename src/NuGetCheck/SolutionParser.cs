using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Build.Construction;

namespace NuGetCheck
{
	public class SolutionParser
	{
		private static readonly Type SolutionParserType = GetSolutionParserType();
		private static readonly PropertyInfo SolutionReaderProperty = GetSolutionReaderProperty();
		private static readonly MethodInfo ParseSolutionMethod = GetParseSolutionMethod();
		private static readonly PropertyInfo ProjectsProperty = GetProjectsProperty();

		public List<string> GetAllProjectFileNames(string solutionFileName)
		{
			var solutionParser = SolutionParserType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null).Invoke(null);
			using (var streamReader = new StreamReader(File.OpenRead(solutionFileName)))
			{
				SolutionReaderProperty.SetValue(solutionParser, streamReader, null);
				ParseSolutionMethod.Invoke(solutionParser, null);
			}
			var projects = new List<string>();
			foreach (var proj in (object[])ProjectsProperty.GetValue(solutionParser, null))
			{
				var projectInSolution = new ProjectInSolution(proj);
				if (!projectInSolution.IsSolutionFolder)
				{
					projects.Add(Path.Combine(Path.GetDirectoryName(solutionFileName), projectInSolution.RelativePath));
				}
			}
			return projects;
		}

		private static Type GetSolutionParserType()
		{
			var assembly = typeof(ProjectElement).Assembly;
			return assembly.GetType("Microsoft.Build.Construction.SolutionParser");
		}

		private static PropertyInfo GetSolutionReaderProperty()
		{
			if (SolutionParserType != null)
			{
				return SolutionParserType.GetProperty("SolutionReader", BindingFlags.NonPublic | BindingFlags.Instance);
			}

			return null;
		}

		private static MethodInfo GetParseSolutionMethod()
		{
			if (SolutionParserType != null)
			{
				return SolutionParserType.GetMethod("ParseSolution", BindingFlags.NonPublic | BindingFlags.Instance);
			}

			return null;
		}

		private static PropertyInfo GetProjectsProperty()
		{
			if (SolutionParserType != null)
			{
				return SolutionParserType.GetProperty("Projects", BindingFlags.NonPublic | BindingFlags.Instance);
			}

			return null;
		}

		public class ProjectInSolution
		{
			private static readonly Type ProjectInSolutionType = GetProjectInSolutionType();
			private static readonly PropertyInfo RelativePathProperty = GetRelativePathProperty();
			private static readonly PropertyInfo ProjectTypeProperty = GetProjectTypeProperty();

			public string RelativePath { get; private set; }

			public bool IsSolutionFolder { get; private set; }

			public ProjectInSolution(object solutionProject)
			{
				string projectType = ProjectTypeProperty.GetValue(solutionProject, index: null).ToString();
				IsSolutionFolder = projectType.Equals("SolutionFolder", StringComparison.OrdinalIgnoreCase);
				RelativePath = (string)RelativePathProperty.GetValue(solutionProject, index: null);
			}

			private static Type GetProjectInSolutionType()
			{
				var assembly = typeof(Microsoft.Build.Construction.ProjectElement).Assembly;
				return assembly.GetType("Microsoft.Build.Construction.ProjectInSolution");
			}

			private static PropertyInfo GetRelativePathProperty()
			{
				return ProjectInSolutionType.GetProperty("RelativePath", BindingFlags.NonPublic | BindingFlags.Instance);
			}

			private static PropertyInfo GetProjectTypeProperty()
			{
				return ProjectInSolutionType.GetProperty("ProjectType", BindingFlags.NonPublic | BindingFlags.Instance);
			}
		}
	}
}
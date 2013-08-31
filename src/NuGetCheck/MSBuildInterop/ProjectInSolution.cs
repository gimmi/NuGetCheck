using System;
using System.Reflection;
using Microsoft.Build.Construction;

namespace NuGetCheck.MSBuildInterop
{
	public class ProjectInSolution
	{
		private readonly object _instance;

		public ProjectInSolution(object instance)
		{
			_instance = instance;
		}

		public string RelativePath
		{
			get { return (string) RelativePathProperty.GetValue(_instance, null); }
		}

		public string ProjectName
		{
			get { return (string) ProjectNameProperty.GetValue(_instance, null); }
		}

		public bool IsSolutionFolder
		{
			get { return ProjectTypeProperty.GetValue(_instance, null).ToString().Equals("SolutionFolder", StringComparison.OrdinalIgnoreCase); }
		}

		private static Type ProjectInSolutionType
		{
			get { return typeof (ProjectElement).Assembly.GetType("Microsoft.Build.Construction.ProjectInSolution"); }
		}

		private static PropertyInfo RelativePathProperty
		{
			get { return ProjectInSolutionType.GetProperty("RelativePath", BindingFlags.NonPublic | BindingFlags.Instance); }
		}

		private static PropertyInfo ProjectNameProperty
		{
			get { return ProjectInSolutionType.GetProperty("ProjectName", BindingFlags.NonPublic | BindingFlags.Instance); }
		}

		private static PropertyInfo ProjectTypeProperty
		{
			get { return ProjectInSolutionType.GetProperty("ProjectType", BindingFlags.NonPublic | BindingFlags.Instance); }
		}
	}
}
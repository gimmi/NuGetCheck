using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Construction;

namespace NuGetCheck.MSBuildInterop
{
	public class SolutionParser
	{
		private readonly object _instance;

		public SolutionParser()
		{
			_instance = ConstructorInfo.Invoke(null);
		}

		public ProjectInSolution[] Projects
		{
			get { return ((IEnumerable<object>) ProjectsProperty.GetValue(_instance, null)).Select(x => new ProjectInSolution(x)).ToArray(); }
		}

		public StreamReader SolutionReader
		{
			set { SolutionReaderProperty.SetValue(_instance, value, null); }
		}

		private static ConstructorInfo ConstructorInfo
		{
			get { return SolutionParserType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null); }
		}

		private static Type SolutionParserType
		{
			get { return typeof (ProjectElement).Assembly.GetType("Microsoft.Build.Construction.SolutionParser"); }
		}

		private static PropertyInfo SolutionReaderProperty
		{
			get { return SolutionParserType.GetProperty("SolutionReader", BindingFlags.NonPublic | BindingFlags.Instance); }
		}

		private static MethodInfo ParseSolutionMethod
		{
			get { return SolutionParserType.GetMethod("ParseSolution", BindingFlags.NonPublic | BindingFlags.Instance); }
		}

		private static PropertyInfo ProjectsProperty
		{
			get { return SolutionParserType.GetProperty("Projects", BindingFlags.NonPublic | BindingFlags.Instance); }
		}

		public void ParseSolution()
		{
			ParseSolutionMethod.Invoke(_instance, null);
		}
	}
}
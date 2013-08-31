using System;
using System.IO;
using System.Text;
using NuGetCheck.MSBuildInterop;
using NUnit.Framework;
using SharpTestsEx;

namespace NuGetCheck.Tests
{
	[TestFixture]
	public class PackageVersionMismatchCommandTest
	{
		[SetUp]
		public void SetUp()
		{
			_sut = new PackageVersionMismatchCommand(new SolutionParser(), new PackagesConfigParser());
		}

		private PackageVersionMismatchCommand _sut;

		[Test]
		public void Should_reveal_version_mismatch_in_solution()
		{
			var sb = new StringBuilder();
			var returnValue = _sut.Run(new[] {Path.Combine("TestSolution", "TestSolution.sln")}, new StringWriter(sb));

			returnValue.Should().Be.EqualTo(1);
			var actualMessages = sb.ToString().Split(new []{ Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

			actualMessages.Should().Have.SameSequenceAs(new[] {
				@"Checking solution TestSolution\TestSolution.sln",
				@"Checking 3 projects",
				@"Package log4net.1.2.10 is referenced in ClassLibrary2",
				@"Package log4net.2.0.0 is referenced in ClassLibrary1",
				@"Solution contain conflicting packages, see previous messages."
			});
		}
	}
}
using System.IO;

namespace NuGetCheck
{
	public interface ICommand
	{
		int Run(string[] args, TextWriter log);
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NuGetCheck.MSBuildInterop;

namespace NuGetCheck
{
	public class Program
	{
		public static Dictionary<string, ICommand> Commands = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase) {
			{"PackageVersionMismatch", new PackageVersionMismatchCommand(new SolutionParser(), new PackagesConfigParser()) }
		};

		public static int Main(string[] args)
		{
			try
			{
				return Run(args, Console.Out);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return 1;
			}
		}

		public static int Run(string[] args, TextWriter log)
		{
			log.WriteLine("{0} {1}", typeof (Program).Assembly.GetCustomAttributes(typeof (AssemblyProductAttribute), false).Cast<AssemblyProductAttribute>().Single().Product, typeof (Program).Assembly.GetCustomAttributes(typeof (AssemblyInformationalVersionAttribute), false).Cast<AssemblyInformationalVersionAttribute>().Single().InformationalVersion);
			log.WriteLine(typeof (Program).Assembly.GetCustomAttributes(typeof (AssemblyDescriptionAttribute), false).Cast<AssemblyDescriptionAttribute>().Single().Description);
			log.WriteLine(typeof (Program).Assembly.GetCustomAttributes(typeof (AssemblyCopyrightAttribute), false).Cast<AssemblyCopyrightAttribute>().Single().Copyright);

			if (args.Length == 0)
			{
				log.Write("No command specified. Available commands: ");
				log.WriteLine(string.Join(", ", Commands.Keys));
				return 1;
			}

			log.WriteLine();
			var commandName = args[0];
			log.WriteLine("Executing command {0}", commandName);

			var command = Commands[commandName];
			return command.Run(args.Skip(1).ToArray(), log);
		}
	}
}
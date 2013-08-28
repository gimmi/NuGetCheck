using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NuGetCheck
{
	public class Program
	{
		public static Dictionary<string, ICommand> Commands = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase) {
			{"PackageVersionMismatch", new PackageVersionMismatchCommand(new SolutionParser()) }
		};

		public static int Main(string[] args)
		{
			try
			{
				Run(args, Console.Out);
				Console.WriteLine("Done");
				return 0;
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
				log.Write("Available commands: ");
				log.WriteLine(string.Join(", ", Commands.Keys));
				return 1;
			}

			var command = Commands[args[0]];
			return command.Run(args.Skip(1).ToArray(), log);
		}
	}
}
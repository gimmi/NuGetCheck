using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NuGetCheck
{
	public static class TextWriterExtensions
	{
		public static void Write(this TextWriter textWriter)
		{
			textWriter.Write();
		}
	}
}

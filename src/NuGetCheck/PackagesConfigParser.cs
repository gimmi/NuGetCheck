using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace NuGetCheck
{
	public class PackagesConfigParser
	{
		public ICollection<PackageInfo> Parse(string filePath)
		{
			return XDocument.Load(filePath).Element("packages").Descendants("package").Select(x => new PackageInfo {
				Id = x.Attribute("id").Value,
				Version = x.Attribute("version").Value
			}).ToArray();
		} 
	}
}
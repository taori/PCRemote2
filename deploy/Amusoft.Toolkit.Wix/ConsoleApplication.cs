using System.Threading.Tasks;
using Amusoft.Toolkit.Wix.Features;
using CommandDotNet;

namespace Amusoft.Toolkit.Wix
{
	public class ConsoleApplication
	{
		public class Generate
		{
			[Command(
				Description = "Generates *.wxs file based on the files within a directory", 
				Name = "productWxsFromDirectory")]
			public async Task<int> ProductWxsFromDirectory(string directory, string targetWxsPath, string ignoreFile)
			{
				var feature = new GenerateProductWxsFromDirectoryFeature(directory, targetWxsPath, ignoreFile);
				return await feature.ExecuteAsync();
			}
		}
	}
}
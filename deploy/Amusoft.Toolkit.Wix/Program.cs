using System;
using System.Threading.Tasks;
using CommandDotNet;

namespace Amusoft.Toolkit.Wix
{
	class Program
	{
		static async Task<int> Main(string[] args)
		{
			var appRunner = new AppRunner<ConsoleApplication>();

			return await appRunner
				.UseVersionMiddleware()
				.RunAsync();
		}
	}
}

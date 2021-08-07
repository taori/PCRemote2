using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Newtonsoft.Json;
using NLog;
using Xamarin.Essentials;

namespace Amusoft.PCR.Mobile.Droid.Domain.Features.WakeOnLan
{
	public static class WakeOnLanManager
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(WakeOnLanManager));

		public static async Task<MacPackage> GetMacPackageAsync(GrpcApplicationAgent agent)
		{
			Log.Debug("Building Mac package");
			var addresses = await agent.DesktopClient.GetNetworkMacAddressesAsync(TimeSpan.FromSeconds(5));
			var hostName = await agent.DesktopClient.GetHostNameAsync(TimeSpan.FromSeconds(5));
			var macPackage = new MacPackage();
			macPackage.Addresses = addresses.Select(d => d.MacAddress).ToArray();
			macPackage.HostName = hostName;

			Log.Debug("Package created for {HostName}", hostName);
			return macPackage;
		}

		public static async Task<MacPackage[]> GetDefinitionsAsync()
		{
			var path = GetWolClientPath();
			if (!File.Exists(path))
				return Array.Empty<MacPackage>();

			var content = await File.ReadAllTextAsync(path);
			return JsonConvert.DeserializeObject<MacPackage[]>(content);
		}

		public static async Task SaveDefinitionAsync(MacPackage package)
		{
			var packages = (await GetDefinitionsAsync()).ToDictionary(d => d.HostName);
			if (packages.ContainsKey(package.HostName))
			{
				Log.Debug("Updating package for {HostName}", package.HostName);
				packages[package.HostName] = package;
			}
			else
			{
				Log.Debug("Adding package for {HostName}", package.HostName);
				packages.Add(package.HostName, package);
			}

			var path = GetWolClientPath();
			var packagesArray = packages.Select(d => d.Value).ToArray();

			Log.Debug("Saving packages");
			await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(packagesArray));
		}

		private static string GetWolClientPath()
		{
			return Path.Combine(FileSystem.CacheDirectory, "wol-clients.json");
		}
	}
}
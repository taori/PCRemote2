using System;
using System.Collections.Generic;
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

		private static readonly WakeOnLanMacStorage Storage = new WakeOnLanMacStorage();

		public static async Task<MacPackage> GetMacPackageAsync(GrpcApplicationAgent agent)
		{
			Log.Trace("Building Mac package");
			var addresses = await agent.DesktopClient.GetNetworkMacAddressesAsync(TimeSpan.FromSeconds(5));
			var hostName = await agent.DesktopClient.GetHostNameAsync(TimeSpan.FromSeconds(5));
			var macPackage = new MacPackage();
			macPackage.Addresses = addresses.Select(d => d.MacAddress).ToArray();
			macPackage.HostName = hostName;

			Log.Trace("Package created for {HostName}", hostName);
			return macPackage;
		}

		public static async Task<List<MacPackage>> GetDefinitionsAsync()
		{
			return await Storage.LoadAsync();
		}

		public static async Task UpdateDefinitionAsync(MacPackage package)
		{
			Log.Debug("Wake on LAN package updated");
			await Storage.AddOrUpdateAsync(package);
		}
	}
}
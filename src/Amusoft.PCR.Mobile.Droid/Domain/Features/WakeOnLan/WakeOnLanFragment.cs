using System.Collections.Generic;
using System.Threading.Tasks;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Helpers;
using Android.Widget;
using NLog;

namespace Amusoft.PCR.Mobile.Droid.Domain.Features.WakeOnLan
{
	public class WakeOnLanFragment : AsyncButtonListFragment
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(WakeOnLanFragment));

		protected override async Task<List<ButtonElement>> GetButtonsAsync()
		{
			var items = new List<ButtonElement>();
			var storage = await WakeOnLanManager.GetDefinitionsAsync();
			foreach (var package in storage)
			{
				items.Add(new ButtonElement(true, $"Wake {package.HostName}",async () => await ButtonAction(package) ));
			}

			return items;
		}

		private async Task ButtonAction(MacPackage package)
		{
			Log.Info("Waking up host {Name} with {Count} last known addresses", package.HostName, package.Addresses.Length);
			foreach (var address in package.Addresses)
			{
				Log.Debug("Waking up physical address {Id}", address);
				await Amusoft.Toolkit.Networking.WakeOnLan.UsingAddressAsync(address);
			}

			ToastHelper.Display(Context, $"Wake On LAN for {package.HostName} sent", ToastLength.Long);
		}
	}
}
using System.IO;
using Amusoft.PCR.Mobile.Droid.Toolkit;
using Xamarin.Essentials;

namespace Amusoft.PCR.Mobile.Droid.Domain.Features.WakeOnLan
{
	public class WakeOnLanMacStorage : SerializedStorage<MacPackage>
	{
		protected override string GetPath()
		{
			return Path.Combine(FileSystem.AppDataDirectory, "wol-clients.json");
		}

		protected override bool ItemEqual(MacPackage a, MacPackage b)
		{
			return a.HostName == b.HostName;
		}
	}
}
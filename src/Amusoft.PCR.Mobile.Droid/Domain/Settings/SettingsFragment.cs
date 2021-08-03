using System;
using System.Collections.Generic;
using System.IO;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Helpers;
using Android.Widget;
using NLog;
using Xamarin.Essentials;

namespace Amusoft.PCR.Mobile.Droid.Domain.Settings
{
	public class SettingsFragment : StaticButtonListFragment
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(SettingsFragment));

		protected override IEnumerable<ButtonElement> GetButtonElements()
		{
			yield return new ButtonElement(true, "Clear secure storage", ClearStorageClicked);
			yield return new ButtonElement(true, "Delete logs", DeleteLogsClicked);
		}

		private void DeleteLogsClicked()
		{
			try
			{
				var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				var path = Path.Combine(root, "logs", "nlog.csv");
				if (File.Exists(path))
				{
					File.Delete(path);
				}
				ToastHelper.Display(Context, "Logs deleted", ToastLength.Short);
			}
			catch (Exception e)
			{
				ToastHelper.Display(Context, "Failed to clear logs", ToastLength.Short);
				Log.Error(e);
			}
		}

		private void ClearStorageClicked()
		{
			SecureStorage.RemoveAll();
			ToastHelper.Display(Context, "Secure storage cleared", ToastLength.Short);
		}
	}
}
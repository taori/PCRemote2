using System;
using System.Globalization;
using System.Threading.Tasks;
using Android.App;
using AndroidX.Work;
using Xamarin.Essentials;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.SystemStateControl
{
	public enum SystemStateKind
	{
		Shutdown,
		Restart,
		Hibernate
	}

	public static class SystemStateManager
	{
		public static async Task<DateTime?> GetScheduledTimeAsync(string hostName, SystemStateKind kind)
		{
			var value = await SecureStorage.GetAsync($"{hostName}:{kind}");
			if (value == null)
				return null;

			return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
		}

		public static async Task SetScheduledTimeAsync(string hostName, SystemStateKind kind, DateTime date)
		{
			await SecureStorage.SetAsync($"{hostName}:{kind}", date.ToString(CultureInfo.InvariantCulture));
		}

		public static bool Clear(string hostName, SystemStateKind kind)
		{
			return SecureStorage.Remove($"{hostName}:{kind}");
		}

		public static void AbortAllTimers(string address)
		{
			WorkManager.GetInstance(Application.Context).CancelAllWorkByTag(address);
			Clear(address, SystemStateKind.Hibernate);
			Clear(address, SystemStateKind.Restart);
			Clear(address, SystemStateKind.Restart);
		}
	}
}
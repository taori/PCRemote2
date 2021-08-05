using System;
using System.Threading.Tasks;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Android.Content;
using AndroidX.Work;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.SystemStateControl
{
	public class ShutdownWorker : DelayedSystemStateWorker
	{
		public ShutdownWorker(Context context, WorkerParameters workerParams) : base(context, workerParams)
		{
		}

		protected override SystemStateKind GetStateKind()
		{
			return SystemStateKind.Shutdown;
		}

		protected override string GetNotificationTitle(string hostName)
		{
			return $"{GetStateKind()} {hostName}";
		}

		protected override int GetNotificationIcon()
		{
			return Resource.Drawable.outline_power_settings_new_24;
		}

		protected override string GetAbortAction()
		{
			return AbortBroadcastReceiver.ActionKindShutdown;
		}

		protected override string GetSuccessToastMessage(string hostName)
		{
			return $"Shutting down {hostName}";
		}

		protected override NotificationChannelDeclaration GetNotificationChannel()
		{
			return NotificationChannels.Shutdown;
		}

		protected override async Task<bool> ExecuteFinalizerAsync(GrpcApplicationAgent agent)
		{
			return await agent.DesktopClient.ShutDownDelayedAsync(TimeSpan.FromSeconds(5), false, TimeSpan.FromSeconds(60));
		}
	}
}
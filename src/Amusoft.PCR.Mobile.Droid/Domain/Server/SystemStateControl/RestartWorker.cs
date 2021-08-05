using System;
using System.Threading.Tasks;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Android.Content;
using AndroidX.Work;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.SystemStateControl
{
	public class RestartWorker : DelayedSystemStateWorker
	{
		public RestartWorker(Context context, WorkerParameters workerParams) : base(context, workerParams)
		{
		}

		protected override SystemStateKind GetStateKind()
		{
			return SystemStateKind.Restart;
		}

		protected override string GetNotificationTitle(string hostName)
		{
			return $"{GetStateKind()} {hostName}";
		}

		protected override int GetNotificationIcon()
		{
			return Resource.Drawable.outline_restart_alt_white_48;
		}

		protected override string GetAbortAction()
		{
			return AbortBroadcastReceiver.ActionKindRestart;
		}

		protected override string GetSuccessToastMessage(string hostName)
		{
			return $"Restarting {hostName}";
		}

		protected override NotificationChannelDeclaration GetNotificationChannel()
		{
			return NotificationChannels.Restart;
		}

		protected override async Task<bool> ExecuteFinalizerAsync(GrpcApplicationAgent agent)
		{
			return await agent.DesktopClient.RestartAsync(TimeSpan.FromSeconds(5), false, TimeSpan.FromSeconds(60));
		}
	}
}
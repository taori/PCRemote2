using System;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Android.App;
using Android.Content;
using AndroidX.Work;
using Java.Util;
using NLog;
using Logger = NLog.Logger;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.SystemStateControl
{
	[BroadcastReceiver]
	[IntentFilter(new []{ ActionKindHibernate, ActionKindRestart, ActionKindShutdown})]
	public class AbortBroadcastReceiver : BroadcastReceiver
	{
		public static readonly AbortBroadcastReceiver Instance = new AbortBroadcastReceiver();

		private static readonly Logger Log = LogManager.GetLogger(nameof(AbortBroadcastReceiver));

		public const string ActionKindRestart = "Restart";
		public const string ActionKindShutdown = "Shutdown";
		public const string ActionKindHibernate = "Hibernate";
		public const string NotificationIdTag = "NotificationId";
		public const string WorkIdTag = "WorkId";
		public const string HostAddressTag = "HostAddress";

		public override void OnReceive(Context context, Intent intent)
		{
			var hostAddress = intent.GetStringExtra(HostAddressTag);
			var workId = intent.GetStringExtra(WorkIdTag);
			var notificationId = intent.GetIntExtra(NotificationIdTag, 0);
			if (workId != null && notificationId != 0 && hostAddress != null)
			{
				Log.Debug("Aborting work for {Action} {NotificationId} {WorkId}", intent.Action, notificationId, workId);
				try
				{
					WorkManager.GetInstance(Application.Context).CancelWorkById(UUID.FromString(workId));
					NotificationHelper.DestroyNotification(notificationId);

					switch (intent.Action)
					{
						case ActionKindShutdown:
							SystemStateManager.Clear(hostAddress, SystemStateKind.Shutdown);
							break;
						case ActionKindHibernate:
							SystemStateManager.Clear(hostAddress, SystemStateKind.Hibernate);
							break;
						case ActionKindRestart:
							SystemStateManager.Clear(hostAddress, SystemStateKind.Restart);
							break;
					}
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
	}
}
using System;
using System.Threading.Tasks;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Android.Content;
using AndroidX.Work;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.SystemStateControl
{
	public class HibernateWorker : DelayedSystemStateWorker
	{
		public HibernateWorker(Context context, WorkerParameters workerParams) : base(context, workerParams)
		{
		}

		protected override SystemStateKind GetStateKind()
		{
			return SystemStateKind.Hibernate;
		}

		protected override string GetNotificationTitle(string hostName)
		{
			return $"{GetStateKind()} {hostName}";
		}

		protected override int GetNotificationIcon()
		{
			return Resource.Drawable.outline_snooze_white_48;
		}

		protected override string GetAbortAction()
		{
			return AbortBroadcastReceiver.ActionKindHibernate;
		}

		protected override string GetSuccessToastMessage(string hostName)
		{
			return $"Hibernating {hostName}";
		}

		protected override NotificationChannelDeclaration GetNotificationChannel()
		{
			return NotificationChannels.Hibernate;
		}

		protected override async Task<bool> ExecuteFinalizerAsync(GrpcApplicationAgent agent)
		{
			return await agent.DesktopClient.HibernateAsync(TimeSpan.FromSeconds(5));
		}
	}
}
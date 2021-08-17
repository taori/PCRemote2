using System;
using System.Threading;
using System.Threading.Tasks;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Helpers;
using Amusoft.PCR.Mobile.Droid.Toolkit;
using Android.App;
using Android.Content;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Work;
using Grpc.Core;
using NLog;
using Logger = NLog.Logger;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.SystemStateControl
{
	public abstract class DelayedSystemStateWorker : AsyncWorker
	{
		public const string AgentAddressTag = "Address";
		public const string ProgressPercentTag = "Progress";
		public const string ProgressVisibleTag = "Visible";

		private static readonly Logger Log = LogManager.GetLogger(nameof(DelayedSystemStateWorker));

		private readonly string _agentAddress;

		protected abstract SystemStateKind GetStateKind();

		protected abstract string GetNotificationTitle(string hostName);

		protected abstract int GetNotificationIcon();

		protected abstract string GetAbortAction();

		protected abstract string GetSuccessToastMessage(string hostName);

		protected abstract NotificationChannelDeclaration GetNotificationChannel();

		public DelayedSystemStateWorker(Context context, WorkerParameters workerParams) : base(context, workerParams)
		{
			_agentAddress = workerParams.InputData.GetString(AgentAddressTag);
		}

		protected override object GetCompletionTag()
		{
			return _agentAddress + GetStateKind();
		}

		protected override async Task<Data> DoWorkAsync(CancellationToken cancellationToken)
		{
			var notificationId = (GetStateKind() + "+" + _agentAddress).GetHashCode();
			Log.Debug("Spawned notification with Id {Id}", notificationId);
			try
			{
				var scheduled = await SystemStateManager.GetScheduledTimeAsync(_agentAddress, GetStateKind());
				if (scheduled == null || DateTime.Now > scheduled.Value)
				{
					Log.Info("A system state event passed while the device was not working.");
					return Data.Empty;
				}

				var progressDataBuilder = new Data.Builder();

				UpdateProgress(progressDataBuilder, 0, false);
				using (var agent = GrpcApplicationAgentFactory.Create(_agentAddress))
				{
					var hostName = await agent.DesktopClient.GetHostNameAsync(TimeSpan.FromSeconds(5));
					var notification = NotificationHelper.DisplayNotification(notificationId, builder =>
					{
						builder.SetCategory(NotificationCompat.CategoryProgress);
						builder.SetContentTitle(GetNotificationTitle(hostName));
						builder.SetOnlyAlertOnce(true);
						builder.SetSmallIcon(GetNotificationIcon());
						var intent = new Intent();
						intent.SetAction(GetAbortAction());
						intent.PutExtra(AbortBroadcastReceiver.NotificationIdTag, notificationId);
						intent.PutExtra(AbortBroadcastReceiver.WorkIdTag, Id.ToString());
						intent.PutExtra(AbortBroadcastReceiver.HostAddressTag, _agentAddress);
						var pendingIntent = PendingIntent.GetBroadcast(Application.Context, notificationId, intent, PendingIntentFlags.OneShot);
						builder.AddAction(Android.Resource.Drawable.ButtonPlus, "Abort", pendingIntent);

					}, GetNotificationChannel());
					
					SetForegroundAsync(new ForegroundInfo(notificationId, notification.Build()));

					var startDifference = scheduled.Value - DateTime.Now;
					while (scheduled > DateTime.Now)
					{
						var currentDifference = scheduled.Value - DateTime.Now;
						var progress = (100f / startDifference.TotalSeconds) * currentDifference.TotalSeconds;
						notification.SetProgress(100, (int)progress, false);
						UpdateProgress(progressDataBuilder, progress, true);
						notification.SetContentText(currentDifference.ToString("hh\\:mm\\:ss"));

						NotificationHelper.UpdateNotification(ApplicationContext, notificationId, notification);

						await Task.Delay(1000, cancellationToken);
					}

					UpdateProgress(progressDataBuilder, 0, false);
					NotificationHelper.DestroyNotification(notificationId);
					SystemStateManager.Clear(_agentAddress, GetStateKind());

					var result = await ExecuteFinalizerAsync(agent);
					Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
					{
						if (result)
						{
							ToastHelper.Display(GetSuccessToastMessage(hostName), ToastLength.Long);
						}
						else
						{
							ToastHelper.Display("Error", ToastLength.Short);
						}
					});
				}
			}
			catch (RpcException e)
			{
				Log.Debug(e, "Cancelling work because of an RPC exception");
				WorkManager.GetInstance(ApplicationContext).CancelWorkById(Id);
			}

			return Data.Empty;
		}

		private void UpdateProgress(Data.Builder progressDataBuilder, double progress, bool visible)
		{
			progressDataBuilder.PutInt(ProgressPercentTag, (int) progress);
			progressDataBuilder.PutBoolean(ProgressVisibleTag, visible);
			SetProgressAsync(progressDataBuilder.Build());
		}

		protected abstract Task<bool> ExecuteFinalizerAsync(GrpcApplicationAgent agent);
	}
}
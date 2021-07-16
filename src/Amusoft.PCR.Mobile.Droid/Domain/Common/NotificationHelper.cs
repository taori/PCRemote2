using System;
using Amusoft.PCR.Mobile.Droid.Services;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public class NotificationHelper
	{
		public static void DisplayNotification(int notificationId, Action<NotificationCompat.Builder> builder)
		{
			// builder.SetSmallIcon(Resource.Drawable.navigation_empty_icon)
			// 	.SetContentTitle("title")
			// 	.SetContentText("content")
			// 	.SetCategory(NotificationCompat.CategoryProgress)
			// 	.SetProgress(100, 50, false)
			// 	.SetVisibility(NotificationCompat.VisibilityPublic)
			// 	.SetPriority(NotificationCompat.PriorityDefault);

			var currentActivity = ActivityLifecycleCallbacks.Instance.TopMostActivity;
			var notificationManager = currentActivity.GetSystemService(Context.NotificationService) as NotificationManager;

			var builderInstance = new NotificationCompat.Builder(currentActivity, currentActivity.GetString(Resource.String.notification_channel_id));
			builderInstance.SetSmallIcon(Resource.Drawable.outline_power_settings_new_24);
			builder(builderInstance);
				
			notificationManager.Notify(notificationId, builderInstance.Build());
		}

		public static void CreateNotificationChannel(Activity activity)
		{
			// Create the NotificationChannel, but only on API 26+ because
			// the NotificationChannel class is new and not in the support library
			if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
			{
				var name = activity.GetString(Resource.String.notification_channel_name);
				var description = activity.GetString(Resource.String.notification_channel_description);
				var channelId = activity.GetString(Resource.String.notification_channel_id);
				var importance = NotificationImportance.Default;
				var channel = new NotificationChannel(channelId, name, importance);
				channel.Description = description;
					
				var notificationManager = activity.GetSystemService(Context.NotificationService) as NotificationManager;
				notificationManager.CreateNotificationChannel(channel);
			}
		}

		public static void DestroyNotification(int id)
		{
			var currentActivity = ActivityLifecycleCallbacks.Instance.TopMostActivity;
			var notificationManager = currentActivity.GetSystemService(Context.NotificationService) as NotificationManager;
			notificationManager.Cancel(id);
		}
	}
}
using System;
using Amusoft.PCR.Mobile.Droid.Services;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using AndroidX.Core.App;
using Java.Lang;
using Java.Nio.Charset;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public static class NotificationChannels
	{
		public static readonly NotificationChannelDeclaration General = 
			new NotificationChannelDeclaration(
				Application.Context.GetString(Resource.String.notification_channel_general_id),
				Application.Context.GetString(Resource.String.notification_channel_general_id),
				Application.Context.GetString(Resource.String.notification_channel_general_id),
				NotificationImportance.Default);

		public static readonly NotificationChannelDeclaration Shutdown = 
			new NotificationChannelDeclaration(
				Application.Context.GetString(Resource.String.notification_channel_shutdown_id),
				Application.Context.GetString(Resource.String.notification_channel_shutdown_id),
				Application.Context.GetString(Resource.String.notification_channel_shutdown_id),
				NotificationImportance.Default);

		public static readonly NotificationChannelDeclaration Restart = 
			new NotificationChannelDeclaration(
				Application.Context.GetString(Resource.String.notification_channel_restart_id),
				Application.Context.GetString(Resource.String.notification_channel_restart_id),
				Application.Context.GetString(Resource.String.notification_channel_restart_id),
				NotificationImportance.Default);

		public static readonly NotificationChannelDeclaration Hibernate = 
			new NotificationChannelDeclaration(
				Application.Context.GetString(Resource.String.notification_channel_hibernate_id),
				Application.Context.GetString(Resource.String.notification_channel_hibernate_id),
				Application.Context.GetString(Resource.String.notification_channel_hibernate_id),
				NotificationImportance.Default);
	}

	public class NotificationChannelDeclaration
	{
		public NotificationChannelDeclaration(string id, string name, string description, NotificationImportance importance)
		{
			Id = id;
			Name = name;
			Description = description;
			Importance = importance;
		}

		public string Id { get; }
		public string Name { get; }
		public string Description { get; }
		public NotificationImportance Importance { get; }
	}

	public class NotificationHelper
	{
		public static void UpdateNotification(Context context, int notificationId, NotificationCompat.Builder builder)
		{
			var notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;
			notificationManager.Notify(notificationId, builder.Build());
		}

		public static NotificationCompat.Builder DisplayNotification(int notificationId, Action<NotificationCompat.Builder> builder, NotificationChannelDeclaration channel)
		{
			var currentActivity = ActivityLifecycleCallbacks.Instance.TopMostActivity;
			var notificationManager = currentActivity.GetSystemService(Context.NotificationService) as NotificationManager;

			var builderInstance = new NotificationCompat.Builder(currentActivity, channel.Id);
			builderInstance.SetSmallIcon(Resource.Drawable.outline_power_settings_new_24);
			builderInstance.SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification));
			builder(builderInstance);

			var notification = builderInstance.Build();
			notificationManager.Notify(notificationId, notification);
			return builderInstance;
		}

		public static void SetupNotificationChannels()
		{
			// Create the NotificationChannel, but only on API 26+ because
			// the NotificationChannel class is new and not in the support library
			if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
			{
				var registeredChannels = new[]
				{
					NotificationChannels.General,
					NotificationChannels.Hibernate,
					NotificationChannels.Shutdown,
					NotificationChannels.Restart,
				};

				var notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
				foreach (var channelDeclaration in registeredChannels)
				{
					var channel = new NotificationChannel(channelDeclaration.Id, channelDeclaration.Name, channelDeclaration.Importance) { Description = channelDeclaration.Description };
					notificationManager.CreateNotificationChannel(channel);
				}

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
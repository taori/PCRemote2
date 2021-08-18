using System;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Toolkit.UI;
using Android.Views;
using NLog;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.AudioControl
{
	public class AudioApplicationDataSource : GenericDataSource<AudioFeedResponseItem>
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(AudioApplicationDataSource));

		private readonly GrpcApplicationAgent _agent;

		public AudioApplicationDataSource(GrpcApplicationAgent agent)
		{
			_agent = agent;
		}

		protected override GenericViewHolder<AudioFeedResponseItem> CreateViewHolder(View itemView, int viewType)
		{
			return new AudioApplicationViewHolder(itemView);
		}

		public override async Task ReloadAsync()
		{
			try
			{
				Log.Debug("Loading audio feeds");
				var feeds = await _agent.FullDesktopClient.GetAudioFeedsAsync(new AudioFeedRequest());
				Clear();

				if (feeds.Success)
					UpdateRange(feeds.Items);

				Log.Debug("Feeds loaded. Received {Count} - Success: {Success}", feeds.Items.Count, feeds.Success);
				NotifyDataSetChanged();
			}
			catch (Exception e)
			{
				Log.Error(e);
				NotifyDataSetChanged();
			}
		}

		public override bool IsEqual(AudioFeedResponseItem a, AudioFeedResponseItem b)
		{
			return string.Equals(a.Id, b.Id);
		}
	}
}
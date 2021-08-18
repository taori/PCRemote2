using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Helpers;
using Amusoft.Toolkit.UI;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Google.Protobuf.Collections;
using NLog;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.AudioControl
{
	public class AudioFragment : SmartFragment
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(AudioFragment));

		private readonly GrpcApplicationAgent _agent;
		private SeekBar _seekBar;
		private TextView _textView;
		private Button _button;
		private RecyclerView _recyclerView;
		private SwipeRefreshLayout _swipeRefreshLayout;

		public AudioFragment(GrpcApplicationAgent agent)
		{
			_agent = agent;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resource.Layout.server_control_audio, container, false);
		}

		public override async void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			_seekBar = view.FindViewById<SeekBar>(Resource.Id.seekBar1);
			_textView = view.FindViewById<TextView>(Resource.Id.textView1);
			_button = view.FindViewById<Button>(Resource.Id.button1);
			_recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerView1);
			_swipeRefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout1);

			_swipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
			_recyclerView.SetAdapter(new AudioApplicationDataSource(_agent));

			_button.Click += ToggleMuteClicked;
			_seekBar.ProgressChanged += SeekBarOnProgressChanged;
			_seekBar.SetProgress(await _agent.DesktopClient.GetMasterVolumeAsync(TimeSpan.FromSeconds(5), 45), false);
		}

		public override void OnResume()
		{
			SwipeRefreshLayoutOnRefresh(this, EventArgs.Empty);
			base.OnResume();
		}

		private async void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
		{
			var adapter = _recyclerView.GetAdapter();
			if (adapter is GenericDataSource<AudioFeedResponseItem> dataSource)
			{
				_swipeRefreshLayout.Refreshing = true;
				await dataSource.ReloadAsync();
				_swipeRefreshLayout.Refreshing = false;
			}
		}

		private async void ToggleMuteClicked(object sender, EventArgs e)
		{
			var result = await _agent.DesktopClient.ToggleMuteAsync(TimeSpan.FromSeconds(5));
			switch (result)
			{
				case null:
					ToastHelper.Display("Error", ToastLength.Short);
					break;
				case true:
					ToastHelper.Display("Muted", ToastLength.Short);
					break;
				case false:
					ToastHelper.Display("Unmuted", ToastLength.Short);
					break;
			}
		}

		private void SeekBarOnProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
		{
			const int step = 5;
			var progress = e.Progress;
			progress = progress / step;
			progress = progress * step;
			_textView.Text = $"Master volume: {progress}";
			_seekBar.SetProgress(progress, true);

			Log.Info("Setting master volume to {Value} FromUser: {FromUser}", progress, e.FromUser);
			if (e.FromUser)
			{
				Debouncer.Debounce(nameof(AudioFragment) + nameof(SeekBarOnProgressChanged), async () =>
				{
					await _agent.DesktopClient.SetMasterVolumeAsync(TimeSpan.FromSeconds(5), progress);
					ToastHelper.Display("Volume updated", ToastLength.Short);
				}, TimeSpan.FromSeconds(2));
			}
		}
	}
	
	public abstract class GenericViewHolder<TData> : RecyclerView.ViewHolder
	{
		protected GenericViewHolder(View itemView) : base(new TextView(Application.Context))
		{
		}

		public abstract void BindValues(TData item);
		public abstract void SetupViewReferences();
		public abstract View InflateView(LayoutInflater layoutInflater, ViewGroup parent);
	}

	public abstract class GenericDataSource<TDataItem> : RecyclerView.Adapter
	{
		private List<TDataItem> _items = new List<TDataItem>();

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			if (holder is GenericViewHolder<TDataItem> genericViewHolder)
			{
				genericViewHolder.BindValues(_items[position]);
			}
		}

		protected abstract GenericViewHolder<TDataItem> CreateViewHolder(View itemView, int viewType);

		public sealed override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var viewHolder = CreateViewHolder(null, viewType);
			viewHolder.ItemView = viewHolder.InflateView(LayoutInflater.From(parent.Context), parent);
			viewHolder.SetupViewReferences();
			return viewHolder;
		}

		protected void Clear()
		{
			_items.Clear();
		}

		public override int ItemCount => _items.Count;
		public abstract Task ReloadAsync();

		protected void UpdateRange(IEnumerable<TDataItem> items, bool notify = false)
		{
			_items.Clear();
			_items.AddRange(items);
		}
	}

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
	}

	public class AudioApplicationViewHolder : GenericViewHolder<AudioFeedResponseItem>
	{
		private SeekBar _seekBar;
		private TextView _textView;

		public override void BindValues(AudioFeedResponseItem item)
		{
			_seekBar.Progress = (int) item.Volume;
			_textView.Text = $"{item.Name}: {(int) item.Volume}";
		}

		public override void SetupViewReferences()
		{
			_seekBar = ItemView.FindViewById<SeekBar>(Resource.Id.seekBar1);
			_textView = ItemView.FindViewById<TextView>(Resource.Id.textView1);

			_seekBar.Max = 100;
			_seekBar.Min = 0;
		}

		public override View InflateView(LayoutInflater layoutInflater, ViewGroup parent)
		{
			return layoutInflater.Inflate(Resource.Layout.server_control_audio_item, parent, false);
		}

		public AudioApplicationViewHolder(View itemView) : base(itemView)
		{
		}
	}
}
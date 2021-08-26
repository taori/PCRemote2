using System;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Mobile.Droid.Helpers;
using Amusoft.PCR.Mobile.Droid.Toolkit.UI;
using Amusoft.Toolkit.UI;
using Android.Views;
using Android.Widget;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.AudioControl
{
	public class AudioApplicationViewHolder : GenericViewHolder<AudioFeedResponseItem>
	{
		private SeekBar _seekBar;
		private TextView _textView;
		private ImageView _imageView;
		private bool _muted;
		private int _progress;
		private string _id;

		public override void UpdateFromControls(AudioFeedResponseItem item)
		{
			item.Volume = _progress;
			item.Muted = _muted;
		}

		public override void BindValues(AudioFeedResponseItem item)
		{
			_seekBar.Progress = (int) item.Volume;
			_progress = (int) item.Volume;
			_muted = item.Muted;
			_id = item.Id;
			_textView.Text = $"{item.Name}";

			var resource = _muted ? Resource.Drawable.outline_volume_off_24 : Resource.Drawable.outline_volume_up_24;
			_imageView.SetImageResource(resource);
		}

		public override void SetupViewReferences()
		{
			_seekBar = ItemView.FindViewById<SeekBar>(Resource.Id.seekBar1);
			_textView = ItemView.FindViewById<TextView>(Resource.Id.textView1);
			_imageView = ItemView.FindViewById<ImageView>(Resource.Id.button1);

			_imageView.Clickable = true;
			_imageView.Click += ImageViewOnClick;

			_seekBar.ProgressChanged += SeekBarOnProgressChanged;
			_seekBar.Max = 100;
			_seekBar.Min = 0;
		}

		private void SeekBarOnProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
		{
			if (!e.FromUser)
				return;

			Debouncer.Debounce(nameof(AudioApplicationViewHolder) + nameof(SeekBarOnProgressChanged) + _id, () =>
			{
				_progress = e.Progress;
				RaiseDataUpdateRequired(AbsoluteAdapterPosition);

			}, TimeSpan.FromSeconds(1));
		}

		private void ImageViewOnClick(object sender, EventArgs e)
		{
			_muted = !_muted;
			RaiseDataUpdateRequired(AbsoluteAdapterPosition);
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
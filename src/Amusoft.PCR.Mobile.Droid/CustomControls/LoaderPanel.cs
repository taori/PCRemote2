using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Amusoft.PCR.Mobile.Droid.CustomControls
{
	public class LoaderPanel : FrameLayout
	{
		private FrameLayout _overlay;
		private FrameLayout _contentContainer;

		protected LoaderPanel(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public LoaderPanel(Context context) : base(context)
		{
			InitializeView(context, null);
		}

		public LoaderPanel(Context context, IAttributeSet? attrs) : base(context, attrs)
		{
			InitializeView(context, attrs);
		}

		private void InitializeView(Context context, IAttributeSet? attrs)
		{
			LayoutInflater.FromContext(context).Inflate(Resource.Layout.custom_loader_panel, this);
			_overlay = FindViewById<FrameLayout>(Resource.Id.loading_overlay);
			_contentContainer = FindViewById<FrameLayout>(Resource.Id.content);

			if (attrs == null)
				return;

			var allStyles = context.ObtainStyledAttributes(attrs, Resource.Styleable.LoaderPanel, 0, 0);
			
			OverlayVisible = allStyles.GetBoolean(Resource.Styleable.LoaderPanel_is_panel_visible, false);

			allStyles.Recycle();
		}

		public override void AddView(View? child, int index, ViewGroup.LayoutParams? @params)
		{
			if (_contentContainer == null)
			{
				base.AddView(child, index, @params);
			}
			else
			{
				_contentContainer.AddView(child, _contentContainer.ChildCount - 1, @params);
			}
		}

		public bool OverlayVisible
		{
			get => _overlay.Visibility == ViewStates.Visible;
			set
			{
				_overlay.Visibility = value ? ViewStates.Visible : ViewStates.Invisible;
				Invalidate();
			}
		}
	}
}
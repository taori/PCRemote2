using System;
using System.Numerics;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using NLog;

namespace Amusoft.PCR.Mobile.Droid.CustomControls
{
	public class TrackingView : LinearLayout
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(TrackingView));

		private View _rootView;
		private VelocityTracker _velocityTracker;

		public int Sensitivity { get; set; } = 1000;

		protected TrackingView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public TrackingView(Context context) : base(context)
		{
			InitializeView(context, null);
		}

		public TrackingView(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			InitializeView(context, attrs);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_velocityTracker != null)
				{
					_velocityTracker.Recycle();
					_velocityTracker = null;
				}
			}
			base.Dispose(disposing);
		}

		public event EventHandler<Vector2> VelocityOccured;

		private void InitializeView(Context context, IAttributeSet attrs)
		{
			_rootView = LayoutInflater.FromContext(context).Inflate(Resource.Layout.custom_tracking_view, this);

			if (attrs == null)
				return;

			var allStyles = context.ObtainStyledAttributes(attrs, Resource.Styleable.LoaderPanel, 0, 0);
			allStyles.Recycle();
		}
		
		public override bool DispatchTouchEvent(MotionEvent e)
		{
			var index = e.ActionIndex;
			var action = e.ActionMasked;
			var pointerId = e.GetPointerId(index);

			switch (action)
			{
				case MotionEventActions.Down:
					if (_velocityTracker == null)
					{
						_velocityTracker = VelocityTracker.Obtain();
					}
					else
					{
						// Reset the velocity tracker back to its initial state.
						_velocityTracker.Clear();
					}

					if (IfVelocityTrackerIsNull()) 
						return true;

					_velocityTracker.AddMovement(e);
					break;
				case MotionEventActions.Move:
					if (IfVelocityTrackerIsNull()) 
						return true;

					_velocityTracker.AddMovement(e);
					// When you want to determine the velocity, call
					// computeCurrentVelocity(). Then call getXVelocity()
					// and getYVelocity() to retrieve the velocity for each pointer ID.
					_velocityTracker.ComputeCurrentVelocity(Sensitivity);
					// Log velocity of pixels per second
					// Best practice to use VelocityTrackerCompat where possible.
					var xVel = _velocityTracker.GetXVelocity(pointerId);
					var yVel = _velocityTracker.GetYVelocity(pointerId);
					if(xVel > 0 && yVel > 0)
						VelocityOccured?.Invoke(this, new Vector2(xVel, yVel));

					break;
				case MotionEventActions.Up:
				case MotionEventActions.Cancel:
					if (IfVelocityTrackerIsNull()) 
						return true;

					_velocityTracker.Recycle();
					_velocityTracker = null;
					break;
			}

			return true;
		}

		private bool IfVelocityTrackerIsNull()
		{
			if (_velocityTracker == null)
			{
				Log.Warn("_velocityTracker == null");
				return true;
			}

			return false;
		}
	}
}
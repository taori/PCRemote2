using System;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;

namespace Amusoft.PCR.Mobile.Droid.Toolkit.UI
{
	public abstract class GenericViewHolder<TData> : RecyclerView.ViewHolder
	{
		protected GenericViewHolder(View itemView) : base(new TextView(Application.Context))
		{
		}

		public event EventHandler<int> DataUpdateRequired;

		protected void RaiseDataUpdateRequired(int adapterPosition) =>
			DataUpdateRequired?.Invoke(this, adapterPosition);

		public abstract void UpdateFromControls(TData item);
		public abstract void BindValues(TData item);
		public abstract void SetupViewReferences();
		public abstract View InflateView(LayoutInflater layoutInflater, ViewGroup parent);
	}
}
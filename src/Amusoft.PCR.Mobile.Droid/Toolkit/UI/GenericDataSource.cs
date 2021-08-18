using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using NLog;

namespace Amusoft.PCR.Mobile.Droid.Toolkit.UI
{
	public abstract class GenericDataSource<TDataItem> : RecyclerView.Adapter
	{
		private static readonly Logger Log = LogManager.GetLogger("GenericDataSource");

		private List<TDataItem> _items = new List<TDataItem>();

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			if (holder is GenericViewHolder<TDataItem> genericViewHolder)
			{
				genericViewHolder.BindValues(_items[position]);
			}
		}

		public event EventHandler<TDataItem> UpdateRequired;

		protected abstract GenericViewHolder<TDataItem> CreateViewHolder(View itemView, int viewType);

		public sealed override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var viewHolder = CreateViewHolder(null, viewType);
			viewHolder.DataUpdateRequired += ExecuteUpdateRequired;
			viewHolder.ItemView = viewHolder.InflateView(LayoutInflater.From(parent.Context), parent);
			viewHolder.SetupViewReferences();
			return viewHolder;
		}

		private void ExecuteUpdateRequired(object sender, int i)
		{
			if (sender is GenericViewHolder<TDataItem> viewHolder)
			{
				var currentValue = _items[i];
				viewHolder.UpdateFromControls(currentValue);
				UpdateRequired?.Invoke(this, _items[i]);
			}
			else
			{
				Log.Error("This method should only be raised by a sender which is the original viewholder and of type {Type}", typeof(GenericViewHolder<TDataItem>));
			}
		}

		protected void Clear()
		{
			_items.Clear();
		}

		public override int ItemCount => _items.Count;
		public abstract Task ReloadAsync();
		public abstract bool IsEqual(TDataItem a, TDataItem b);

		public void UpdateRange(IEnumerable<TDataItem> items, bool notify = false)
		{
			_items.Clear();
			_items.AddRange(items);
		}

		public bool UpdateSingle(TDataItem item)
		{
			var index = _items.FindIndex(straw => IsEqual(straw, item));
			if (index < 0)
			{
				Log.Debug("No match found for update");
				return false;
			}

			_items[index] = item;
			NotifyItemChanged(index);
			return true;
		}
	}
}
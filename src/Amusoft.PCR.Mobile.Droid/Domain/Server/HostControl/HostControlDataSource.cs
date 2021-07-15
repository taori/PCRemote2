using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.HostControl
{
	public class HostControlDataSource : RecyclerView.Adapter
	{
		public enum DataSourceLevel
		{
			Top
		}

		public GrpcApplicationAgent Agent { get; set; }

		public class CommandItem
		{
			public string ButtonText { get; set; }

			public Func<Task> ClickCallback { get; set; }
		}

		public HostControlDataSource(IntPtr javaReference, JniHandleOwnership transfer, GrpcApplicationAgent agent) : base(javaReference, transfer)
		{
			Agent = agent;
		}

		public HostControlDataSource(GrpcApplicationAgent agent)
		{
			Agent = agent;
		}

		public void SetupItems(DataSourceLevel level)
		{
			switch (level)
			{
				case DataSourceLevel.Top:

					_items.Add(new CommandItem()
					{
						ButtonText = "Shutdown",
						ClickCallback = CreateWrappedCall(async () => await Agent.DesktopIntegrationClient.ShutDownDelayedAsync(new ShutdownDelayedRequest() { Force = false, Seconds = 60 }))
					});

					_items.Add(new CommandItem()
					{
						ButtonText = "Abort shutdown",
						ClickCallback = CreateWrappedCall(async () => await Agent.DesktopIntegrationClient.AbortShutDownAsync(new AbortShutdownRequest()))
					});

					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(level), level, null);
			}

			NotifyDataSetChanged();
		}

		public event EventHandler<int> CallStarted;
		public event EventHandler<int> CallFinished;
		public event EventHandler<Exception> CallFailed;

		private int _runningCalls;

		private Func<Task> CreateWrappedCall(Func<Task> function)
		{
			return async () =>
			{
				var now = Interlocked.Increment(ref _runningCalls);
				try
				{
					CallStarted?.Invoke(this, now);
					await function();
				}
				catch (Exception e)
				{
					CallFailed?.Invoke(this, e);
				}
				finally
				{
					now = Interlocked.Decrement(ref _runningCalls);
					CallFinished?.Invoke(this, now);
				}
			};
		}

		private List<CommandItem> _items = new List<CommandItem>();

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			if (holder is ServerControlFragmentViewHolder viewHolder)
			{
				viewHolder.ItemClicked -= ViewHolderOnItemClicked;
				viewHolder.ItemClicked += ViewHolderOnItemClicked;
				if (holder.ItemView is Button button)
				{
					button.Text = _items[position].ButtonText;
				}
			}
		}

		private void ViewHolderOnItemClicked(object sender, EventArgs e)
		{
			if (sender is RecyclerView.ViewHolder holder)
			{
				_items[holder.AdapterPosition].ClickCallback?.Invoke();
			}
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var inflatedView = LayoutInflater.FromContext(parent.Context)
				.Inflate(Resource.Layout.server_control_main_item, parent, false);

			return new ServerControlFragmentViewHolder(inflatedView);
		}

		public override int ItemCount => _items.Count;

		private class ServerControlFragmentViewHolder : RecyclerView.ViewHolder
		{
			public ServerControlFragmentViewHolder(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
			{
			}

			public ServerControlFragmentViewHolder(View itemView) : base(itemView)
			{
				itemView.Click += ItemViewOnClick;
			}

			public event EventHandler ItemClicked;

			private void ItemViewOnClick(object sender, EventArgs e)
			{
				ItemClicked?.Invoke(this, e);
			}
		}
	}
}
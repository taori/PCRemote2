using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amusoft.PCR.Mobile.Droid.Domain.Server.AudioControl;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Xamarin.Essentials;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
    public abstract class ButtonListFragment : SmartFragment, SwipeRefreshLayout.IOnRefreshListener
    {
	    private ButtonListDataSource _dataSource;
	    private SwipeRefreshLayout _swipeRefreshLayout;
	    private RecyclerView _recyclerView;

	    protected abstract ButtonListDataSource CreateDataSource();
		
	    public RecyclerView RecyclerView => _recyclerView;
	    public SwipeRefreshLayout SwipeRefreshLayout => _swipeRefreshLayout;

	    public bool DisplayListHeader { get; set; }

		protected ButtonListDataSource DataSource => _recyclerView.GetAdapter() as ButtonListDataSource;

	    protected override void Dispose(bool disposing)
	    {
            _recyclerView.Dispose();
            _swipeRefreshLayout.Dispose();
			_dataSource.Dispose();
		    base.Dispose(disposing);
	    }

	    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
	        return inflater.Inflate(Resource.Layout.button_list, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            _swipeRefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout1);
            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerView1);

			_swipeRefreshLayout.SetOnRefreshListener(this);
			_recyclerView.SetAdapter(GetInitializedDataSource());
        }

        private ButtonListDataSource GetInitializedDataSource()
        {
	        if (_dataSource == null)
	        {
		        _dataSource = CreateDataSource();
		        _dataSource.ReloadAsync().ContinueWith(d => MainThread.BeginInvokeOnMainThread(() => _dataSource.NotifyDataSetChanged()));

				// Task.Run(() => _dataSource.ReloadAsync());
		        return _dataSource;
	        }

	        return _dataSource;
        }

        public async void OnRefresh()
        {
	        await _dataSource.ReloadAsync();
	        _dataSource.NotifyDataSetChanged();
	        _swipeRefreshLayout.Refreshing = false;
        }
    }

    public class ButtonListDataSource : RecyclerView.Adapter
    {
	    private readonly Func<Task<List<ButtonElement>>> _generate;

	    public ButtonListDataSource(Func<Task<List<ButtonElement>>> generate)
	    {
		    _generate = generate ?? throw new ArgumentNullException(nameof(generate));
	    }

	    private List<ButtonElement> _elements = new List<ButtonElement>();

	    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
	    {
		    if (holder is ButtonListViewHolder viewHolder)
		    {
			    viewHolder.ViewHolderClicked -= ViewHolderOnViewHolderClicked;
			    viewHolder.ViewHolderClicked += ViewHolderOnViewHolderClicked;

			    if (holder.ItemView is Button button)
			    {
				    var dataElement = _elements[position];
				    button.Enabled = dataElement.Clickable;
				    button.Text = dataElement.ButtonText;
			    }
		    }
	    }

	    private void ViewHolderOnViewHolderClicked(object sender, int e)
	    {
		    _elements[e].ButtonAction?.Invoke();
	    }

	    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
	    {
		    var view = LayoutInflater.FromContext(parent.Context).Inflate(Resource.Layout.button_list_item, parent, false);
		    return new ButtonListViewHolder(view);
	    }

	    public override int ItemCount => _elements.Count;

	    public void RemoveAt(int index)
	    {
			_elements.RemoveAt(index);
			NotifyItemRemoved(index);
	    }

	    public async Task ReloadAsync()
	    {
			_elements = await _generate?.Invoke();
	    }
    }

    public class ButtonListViewHolder : RecyclerView.ViewHolder
    {
	    public ButtonListViewHolder(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
	    {
	    }

	    public ButtonListViewHolder(View itemView) : base(itemView)
	    {
			itemView.Click += ItemClicked;
	    }

	    public event EventHandler<int> ViewHolderClicked;

	    private void ItemClicked(object sender, EventArgs e)
	    {
		    ViewHolderClicked?.Invoke(this, AbsoluteAdapterPosition);
	    }
    }

    public class ButtonElement
    {
	    public bool Clickable { get; set; }

	    public Action ButtonAction { get; set; }

	    public string ButtonText { get; set; }
    }
}
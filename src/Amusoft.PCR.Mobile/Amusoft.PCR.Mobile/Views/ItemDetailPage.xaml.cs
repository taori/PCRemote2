using Amusoft.PCR.Mobile.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace Amusoft.PCR.Mobile.Views
{
	public partial class ItemDetailPage : ContentPage
	{
		public ItemDetailPage()
		{
			InitializeComponent();
			BindingContext = new ItemDetailViewModel();
		}
	}
}
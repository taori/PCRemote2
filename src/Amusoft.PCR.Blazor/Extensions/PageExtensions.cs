using Microsoft.JSInterop;

namespace Amusoft.PCR.Blazor.Extensions
{
	public class PageExtensions
	{
		private readonly IJSRuntime _jsRuntime;

		public PageExtensions(IJSRuntime jsRuntime)
		{
			_jsRuntime = jsRuntime;
		}

		public void SetTitle(string title) => _jsRuntime.InvokeVoidAsync("Amusoft.Page.setTitle", title);
	}
}
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Amusoft.PCR.Blazor.Extensions
{
	public class HistoryExtensions
	{
		private readonly IJSRuntime _jsRuntime;

		public HistoryExtensions(IJSRuntime jsRuntime)
		{
			_jsRuntime = jsRuntime;
		}

		public ValueTask<string> Back()
		{
			return _jsRuntime.InvokeAsync<string>("Amusoft.History.back");
		}
	}
}
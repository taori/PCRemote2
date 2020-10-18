using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Amusoft.PCR.Blazor.Extensions
{
	public class UIExtensions
	{
		private readonly IJSRuntime _jsRuntime;

		public UIExtensions(IJSRuntime jsRuntime)
		{
			_jsRuntime = jsRuntime;
		}

		public ValueTask<string> Prompt(string message, string watermark = "Type anything here")
		{
			return _jsRuntime.InvokeAsync<string>(
				"Amusoft.Functions.showPrompt",
				message, watermark);
		}

		public ValueTask<string> Alert(string message)
		{
			return _jsRuntime.InvokeAsync<string>(
				"Amusoft.Functions.alert",
				message);
		}
	}
}
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Amusoft.PCR.Blazor.Extensions
{
	public static class JsRuntimeExtensions
	{
		public static ValueTask<string> Prompt(this IJSRuntime jsRuntime, string message, string watermark = "Type anything here")
		{
			return jsRuntime.InvokeAsync<string>(
				"amusoft.showPrompt",
				message, watermark);
		}

		public static ValueTask<string> Alert(this IJSRuntime jsRuntime, string message)
		{
			return jsRuntime.InvokeAsync<string>(
				"amusoft.alert",
				message);
		}
	}
}
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Amusoft.PCR.Blazor.Extensions
{
	public static class JsRuntimeExtensions
	{
		public static ValueTask<string> Prompt(this IJSRuntime jsRuntime, string message, string watermark = "Type anything here")
		{
			return jsRuntime.InvokeAsync<string>(
				"Amusoft.Functions.showPrompt",
				message, watermark);
		}

		public static ValueTask<string> Alert(this IJSRuntime jsRuntime, string message)
		{
			return jsRuntime.InvokeAsync<string>(
				"Amusoft.Functions.alert",
				message);
		}

		public static void Enable(this IJSRuntime jsRuntime, ElementReference elementReference)
		{
			jsRuntime.InvokeVoidAsync(
				"Amusoft.Functions.enable",
				elementReference);
		}

		public static void Disable(this IJSRuntime jsRuntime, ElementReference elementReference)
		{
			jsRuntime.InvokeVoidAsync(
				"Amusoft.Functions.disable",
				elementReference);
		}
	}
}
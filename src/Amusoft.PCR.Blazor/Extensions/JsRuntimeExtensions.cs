using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Amusoft.PCR.Blazor.Extensions
{
	public static class JsRuntimeExtensions
	{
		public static ElementExtensions Element(this IJSRuntime jsRuntime, ElementReference elementReference) => new ElementExtensions(jsRuntime, elementReference);

		public static PageExtensions Page(this IJSRuntime jsRuntime) => new PageExtensions(jsRuntime);

		public static UIExtensions UI(this IJSRuntime jsRuntime) => new UIExtensions(jsRuntime);
	}
}
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Amusoft.PCR.Blazor.Extensions
{
	public class ElementExtensions
	{
		private readonly IJSRuntime _jsRuntime;
		private readonly ElementReference _elementReference;

		public ElementExtensions(IJSRuntime jsRuntime, ElementReference elementReference)
		{
			_jsRuntime = jsRuntime;
			_elementReference = elementReference;
		}

		public void Enable()
		{
			_jsRuntime.InvokeVoidAsync(
				"Amusoft.Functions.enable",
				_elementReference);
		}

		public void Disable()
		{
			_jsRuntime.InvokeVoidAsync(
				"Amusoft.Functions.disable",
				_elementReference);
		}
	}
}
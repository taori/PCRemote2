using System;
using Amusoft.PCR.Blazor.Extensions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Amusoft.PCR.Blazor.Components
{
	public partial class SampleComponent
	{
		[Inject]
		public IJSRuntime Runtime { get; set; }

		private async Task Test(DateTime current)
		{
			var promptResult = await this.Runtime.Prompt($"Is the current time actually {current}?");
			await this.Runtime.Alert(promptResult);
		}
	}
}
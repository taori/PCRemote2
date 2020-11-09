using System.Threading.Tasks;
using Amusoft.PCR.Blazor.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.JSInterop;

namespace Amusoft.PCR.Server.Dependencies
{
	public interface IDialogService
	{
		ValueTask<string> Error(string message);
	}

	public class DialogService : IDialogService
	{
		private readonly IJSRuntime _runtime;

		public DialogService(IJSRuntime runtime)
		{
			_runtime = runtime;
		}

		public ValueTask<string> Error(string message)
		{
			return _runtime.UI().Alert(message);
		}
	}
}
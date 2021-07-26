using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Helpers;
using Android.Content;
using Android.Widget;
using AndroidX.Core.Content;
using Grpc.Core;
using NLog;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.InputControl
{
	public class ClipboardFragment : ButtonListFragment
	{
		private readonly GrpcApplicationAgent _agent;

		private static readonly Logger Log = LogManager.GetLogger(nameof(ClipboardFragment));

		public ClipboardFragment(GrpcApplicationAgent agent)
		{
			_agent = agent;
		}

		protected override ButtonListDataSource CreateDataSource()
		{
			return new ButtonListDataSource(Generate);
		}

		private Task<List<ButtonElement>> Generate()
		{
			var buttons = new List<ButtonElement>();
			buttons.Add(new ButtonElement(true, "Get host clipboard", async () => await GetHostClipboard()));
			buttons.Add(new ButtonElement(true, "Set host clipboard", async () => await SetHostClipboard()));
#if DEBUG
			buttons.Add(new ButtonElement(true, "Tell clipboard", async () => await TellClipboard()));
#endif

			return Task.FromResult(buttons);
		}

		private async Task TellClipboard()
		{
			if (TryGetClipboardContent(out var content))
			{
				ToastHelper.Display(Context, content, ToastLength.Long);
			}
		}

		private async Task SetHostClipboard()
		{
			if (!TryGetClipboardContent(out var content))
			{
				ToastHelper.Display(Context, $"Clipboard cannot be sent", ToastLength.Long);
				return;
			}

			try
			{
				
				var result = await _agent.DesktopClient.SetClipboardAsync(TimeSpan.FromMinutes(1), GetRemoteName(), content);
				ToastHelper.Display(Context, result ? "success" : "error", ToastLength.Short);
			}
			catch (RpcException e) when (e.StatusCode == StatusCode.PermissionDenied)
			{
				ToastHelper.Display(Context, e.Message, ToastLength.Long);
			}
		}

		private bool TryGetClipboardContent(out string content)
		{
			content = default;
			var cm = Context.GetSystemService(Context.ClipboardService) as ClipboardManager;
			if (cm?.PrimaryClipDescription == null)
				return false;
			if (cm?.PrimaryClip == null)
				return false;
			if (!cm.PrimaryClipDescription.HasMimeType(ClipDescription.MimetypeTextPlain))
				return false;
			
			var clipboardItem = cm.PrimaryClip.GetItemAt(0);
			if (clipboardItem == null)
				return false;

			content = clipboardItem.Text;
			return true;
		}

		private async Task GetHostClipboard()
		{
			var cm = Context.GetSystemService(Context.ClipboardService) as ClipboardManager;
			if (cm == null)
				return;

			try
			{
				var content = await _agent.DesktopClient.GetClipboardAsync(TimeSpan.FromMinutes(1), GetRemoteName());
				cm.PrimaryClip = ClipData.NewPlainText("Host clipboard", content);
			}
			catch (RpcException e) when (e.StatusCode == StatusCode.PermissionDenied)
			{
				ToastHelper.Display(Context, e.Message, ToastLength.Long);
			}
		}

		private static string GetRemoteName()
		{
			return Xamarin.Essentials.DeviceInfo.Name;
		}
	}
}
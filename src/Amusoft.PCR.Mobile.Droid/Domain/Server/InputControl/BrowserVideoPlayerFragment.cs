using System.Collections.Generic;
using System.Threading.Tasks;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Helpers;
using Android.Widget;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.InputControl
{
	public class BrowserVideoPlayerFragment : ButtonListAgentFragment
	{
		protected override ButtonListDataSource CreateDataSource()
		{
			return new ButtonListDataSource(Generate);
		}

		private Task<List<ButtonElement>> Generate()
		{
			var buttonElements = new List<ButtonElement>();
			buttonElements.Add(new ButtonElement(true, "Toggle pause", async () => await TogglePauseClicked()));
			buttonElements.Add(new ButtonElement(true, "Escape", async () => await EscapeClicked()));
			buttonElements.Add(new ButtonElement(true, "Fullscreen", async () => await FullScreenClicked()));
			buttonElements.Add(new ButtonElement(true, "Backward", async () => await BackwardClicked()));
			buttonElements.Add(new ButtonElement(true, "Forward", async () => await ForwardClicked()));
			buttonElements.Add(new ButtonElement(true, "Volume +", async () => await VolumeUpClicked()));
			buttonElements.Add(new ButtonElement(true, "Volume -", async() => await VolumeDownClicked()));
			buttonElements.Add(new ButtonElement(true, "Mute", async() => await MuteClicked()));
			return Task.FromResult(buttonElements);
		}

		private async Task<bool> TrySendInput(string message)
		{
#if DEBUG
			await Task.Delay(3000);
#endif

			if (await this.GetAgent().DesktopClient.SendKeysAsync(message))
			{
				ToastHelper.DisplaySuccess(true, ToastLength.Short);
				return true;
			}
			else
			{
				ToastHelper.DisplaySuccess(false, ToastLength.Long);
				return false;
			}
		}

		private async Task MuteClicked()
		{
			await TrySendInput("{m}");
		}

		private async Task VolumeDownClicked()
		{
			await TrySendInput("{DOWN}");
		}

		private async Task VolumeUpClicked()
		{
			await TrySendInput("{UP}");
		}

		private async Task ForwardClicked()
		{
			await TrySendInput("{RIGHT}");
		}

		private async Task BackwardClicked()
		{
			await TrySendInput("{LEFT}");
		}

		private async Task FullScreenClicked()
		{
			await TrySendInput("{f}");
		}

		private async Task EscapeClicked()
		{
			await TrySendInput("{ESC}");
		}

		private async Task TogglePauseClicked()
		{
			await TrySendInput(" ");
		}
	}
}
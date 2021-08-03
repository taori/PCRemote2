using System.Collections.Generic;
using System.Threading.Tasks;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Domain.Server.SystemStateControl;
using Amusoft.PCR.Mobile.Droid.Extensions;
using Android.OS;
using Android.Views;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.InputControl
{
	public class InputFragment : ButtonListFragment
	{
		private readonly GrpcApplicationAgent _agent;

		public InputFragment(GrpcApplicationAgent agent)
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
			buttons.Add(new ButtonElement(true, "Send input", InputClicked));
			buttons.Add(new ButtonElement(true, "Clipboard", ClipboardClicked));
			return Task.FromResult(buttons);
		}

		private void InputClicked()
		{
			using (var transaction = ParentFragmentManager.BeginTransaction())
			{
				transaction.SetStatusBarTitle("Send input");
				transaction.ReplaceContentAnimated(new SendInputFragment(_agent));
				transaction.Commit();
			}
		}

		private void ClipboardClicked()
		{
			using (var transaction = ParentFragmentManager.BeginTransaction())
			{
				transaction.SetStatusBarTitle("Clipboard");
				transaction.ReplaceContentAnimated(new ClipboardFragment(_agent));
				transaction.Commit();
			}
		}
	}
}
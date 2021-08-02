using System.Collections.Generic;
using System.Threading.Tasks;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Extensions;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.InputControl
{
	public class SendInputFragment : ButtonListFragment
	{
		private readonly GrpcApplicationAgent _agent;

		public SendInputFragment(GrpcApplicationAgent agent)
		{
			_agent = agent;
		}

		protected override ButtonListDataSource CreateDataSource()
		{
			return new ButtonListDataSource(Generate);
		}

		private Task<List<ButtonElement>> Generate()
		{
			var buttonElements = new List<ButtonElement>();
			buttonElements.Add(new ButtonElement(true, "Browser video player", BrowserVideoPlayerClicked));
			return Task.FromResult(buttonElements);
		}

		private void BrowserVideoPlayerClicked()
		{
			using (var transaction = ParentFragmentManager.BeginTransaction())
			{
				transaction.SetStatusBarTitle("Browser Video");
				transaction.ReplaceContentAnimated(new BrowserVideoPlayerFragment(_agent));
				transaction.Commit();
			}
		}
	}
}
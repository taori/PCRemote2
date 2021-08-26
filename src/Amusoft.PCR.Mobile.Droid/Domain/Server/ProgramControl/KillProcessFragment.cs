using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Extensions;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.ProgramControl
{
	public class KillProcessFragment : ButtonListAgentFragment
	{
		protected override ButtonListDataSource CreateDataSource()
		{
			return new ButtonListDataSource(GenerateElements);
		}

		private async Task<List<ButtonElement>> GenerateElements()
		{
			var list = new List<ButtonElement>();
			var processes = await this.GetAgent().DesktopClient.GetProcessListAsync(TimeSpan.FromSeconds(5));
			var filtered = processes.GroupBy(d => d.ProcessName).Select(d => d.First());
			AddElements(list, filtered);
			return list;
		}

		private void AddElements(List<ButtonElement> list, IEnumerable<ProcessListResponseItem> filtered)
		{
			foreach (var item in filtered.OrderBy(d => d.ProcessName))
			{
				var buttonElement = new ButtonElement();
				buttonElement.Clickable = true;
				buttonElement.ButtonText = item.ProcessName;
				buttonElement.ButtonAction = () =>
				{
					using (var transaction = ParentFragmentManager.BeginTransaction())
					{
						transaction.ReplaceContentAnimated(new KillProcessByIdFragment(item.ProcessName).WithAgent(this));
						transaction.SetStatusBarTitle($"Process: {item.ProcessName}");
						transaction.Commit();
					}
				};

				list.Add(buttonElement);
			}
		}
	}
}
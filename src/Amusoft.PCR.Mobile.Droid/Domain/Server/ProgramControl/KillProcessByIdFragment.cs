using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Extensions;
using Amusoft.PCR.Mobile.Droid.Helpers;
using Android.Widget;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.ProgramControl
{
	public class KillProcessByIdFragment : ButtonListFragment
	{
		private readonly GrpcApplicationAgent _agent;
		private readonly string _itemProcessName;

		public KillProcessByIdFragment(GrpcApplicationAgent agent, string itemProcessName)
		{
			_agent = agent;
			_itemProcessName = itemProcessName;
		}

		protected override ButtonListDataSource CreateDataSource()
		{
			return new ButtonListDataSource(GenerateElements);
		}

		private async Task<List<ButtonElement>> GenerateElements()
		{
			var list = new List<ButtonElement>();
			var processes = await _agent.DesktopClient.GetProcessListAsync(TimeSpan.FromSeconds(5));
			var filtered = processes.GroupBy(d => d.ProcessName).Select(d => d.First());
			AddElements(list, filtered);
			return list;
		}

		private void AddElements(List<ButtonElement> list, IEnumerable<ProcessListResponseItem> filtered)
		{
			var index = 0;
			foreach (var item in filtered.Where(d => d.ProcessName.Contains(_itemProcessName)).OrderBy(d => d.ProcessId))
			{
				var buttonElement = new ButtonElement();
				buttonElement.Clickable = true;
				buttonElement.ButtonText = $"Kill {item.ProcessId}";
				var scopedIndex = index;
				buttonElement.ButtonAction = async () =>
				{
					if (await _agent.DesktopClient.KillProcessByIdAsync(TimeSpan.FromSeconds(5), item.ProcessId))
					{
						this.DataSource.RemoveAt(scopedIndex);
						ToastHelper.DisplaySuccess(Context, true, ToastLength.Short);
					}
					else
					{
						ToastHelper.DisplaySuccess(Context, false, ToastLength.Short);
					}
				};

				list.Add(buttonElement);

				index++;
			}
		}
	}
}
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Helpers;
using Android.Widget;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.ProgramControl
{
	public class LaunchProgramFragment : ButtonListAgentFragment
	{
		protected override ButtonListDataSource CreateDataSource()
		{
			return new ButtonListDataSource(GenerateData);
		}

		private async Task<List<ButtonElement>> GenerateData()
		{
			using var agent = this.GetAgent();
			var hostCommands = await agent.DesktopClient.GetHostCommandsAsync(TimeSpan.FromSeconds(5));
			var results = new List<ButtonElement>();
			foreach (var command in hostCommands)
			{
				results.Add(new ButtonElement()
				{
					Clickable = true,
					ButtonText = command.Title,
					ButtonAction = async () =>
					{
						var result = await agent.DesktopClient.InvokeHostCommand(TimeSpan.FromSeconds(5), command.CommandId);
						ToastHelper.DisplaySuccess(result, ToastLength.Short);
					}
				});
			}

			if (hostCommands.Count == 0)
			{
				results.Add(new ButtonElement()
				{
					ButtonText = "No commands available",
					ButtonAction = () => {}
				});
			}
			return results;
		}
	}
}
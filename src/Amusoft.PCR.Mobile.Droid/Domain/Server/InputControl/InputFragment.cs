﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Domain.Server.SystemStateControl;
using Amusoft.PCR.Mobile.Droid.Extensions;
using Android.OS;
using Android.Views;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.InputControl
{
	public class InputFragment : ButtonListAgentFragment
	{
		protected override ButtonListDataSource CreateDataSource()
		{
			return new ButtonListDataSource(Generate);
		}

		private Task<List<ButtonElement>> Generate()
		{
			var buttons = new List<ButtonElement>();
			buttons.Add(new ButtonElement(true, "Send input", InputClicked));
			buttons.Add(new ButtonElement(true, "Clipboard", ClipboardClicked));
			buttons.Add(new ButtonElement(true, "Control Mousecursor", CursorControlClicked));
			return Task.FromResult(buttons);
		}

		private void CursorControlClicked()
		{
			using (var transaction = ParentFragmentManager.BeginTransaction())
			{
				transaction.SetStatusBarTitle("Mouse control");
				transaction.ReplaceContentAnimated(new MouseInputFragment().WithAgent(this));
				transaction.Commit();
			}
		}

		private void InputClicked()
		{
			using (var transaction = ParentFragmentManager.BeginTransaction())
			{
				transaction.SetStatusBarTitle("Send input");
				transaction.ReplaceContentAnimated(new SendInputFragment().WithAgent(this));
				transaction.Commit();
			}
		}

		private void ClipboardClicked()
		{
			using (var transaction = ParentFragmentManager.BeginTransaction())
			{
				transaction.SetStatusBarTitle("Clipboard");
				transaction.ReplaceContentAnimated(new ClipboardFragment().WithAgent(this));
				transaction.Commit();
			}
		}
	}
}
using System;
using System.Linq;
using Amusoft.PCR.Domain.VoiceRecognition;
using FluentAssertions;
using Xunit;

namespace Amusoft.PCR.Domain.UnitTests
{
	public class VoiceCommandRunnerTests
	{
		[Fact]
		public void Test1()
		{
			// var expanderValues = new[] {("{Trigger} {Audio} {Switch} {Application}", "")};
			var expanderValues = new[]
			{
				("{Trigger}", "Computer"),
				("{Trigger}", "Trigger"),
				("{Audio}", "Audio"),
				("{Audio}", "Trigger2"),
				("{Switch}", "on"),
				("{Switch}", "an"),
				("{Switch}", "off"),
				("{Application}", "internet"),
				("{Application}", "firefox"),
			};
			var expanderLookup = expanderValues.ToLookup(d => d.Item1, d => d.Item2);
			var commandRegister = new VoiceCommandRegister();
			commandRegister.UseCommandProviders(new AudioCommandProvider());
			commandRegister.UseCommandExpanders(new AudioLookupExpander(expanderLookup));

			VoiceCommandRunner.TryExecute("computer audio on firefox", commandRegister).Should().BeTrue();
		}
	}
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Amusoft.PCR.Domain.VoiceRecognition;
using FluentAssertions;
using Moq;
using NLog;
using Xunit;
using Xunit.Abstractions;

namespace Amusoft.PCR.Domain.UnitTests
{
	public class VoiceCommandRunnerTests : TestBase
	{
		public VoiceCommandRunnerTests(ITestOutputHelper outputHelper) : base(outputHelper)
		{
		}

		[Fact]
		public void VerifyEvaluation()
		{
			var expanderValues = new[]
			{
				("{Trigger}", "Computer"),
				("{Trigger}", "Trigger"),
				("{AudioTrigger}", "Audio"),
				("{AudioTrigger}", "Trigger2"),
				("{On}", "on"),
				("{On}", "an"),
				("{Off}", "off"),
				("{Master}", "master"),
				("{Application}", "internet"),
				("{Application}", "firefox"),
			};
			var expanderLookup = expanderValues.ToLookup(d => d.Item1, d => d.Item2);
			var commandRegister = new VoiceCommandRegister();
			commandRegister.UseCommandProviders(new AudioCommandProvider());
			commandRegister.UseCommandExpanders(
				new AudioApplicationOnExpander(expanderLookup),
				new AudioApplicationOffExpander(expanderLookup),
				new AudioMasterOnExpander(expanderLookup),
				new AudioMasterOffExpander(expanderLookup)
			);

			var commandBinderMock = new Mock<IVoiceCommandBinder>();
			commandBinderMock.Setup(d => d.CanHandle(It.IsAny<ExpandedVoiceCommand>())).Returns(true);
			commandBinderMock.Setup(d => d.ExecuteAsync(It.IsAny<ExpandedVoiceCommand>())).Returns(Task.CompletedTask);

			var commandRunner = new VoiceCommandRunner();
			commandRunner.UseCommandBinders(commandBinderMock.Object);
			commandRunner.TryExecute("computer audio on firefox", commandRegister).Should().BeTrue();

			commandBinderMock.Verify(d => d.ExecuteAsync(It.IsAny<ExpandedVoiceCommand>()), Times.Once);
		}
	}
}

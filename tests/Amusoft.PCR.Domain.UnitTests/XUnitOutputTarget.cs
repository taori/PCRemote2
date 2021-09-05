using NLog;
using NLog.Targets;
using Xunit.Abstractions;

namespace Amusoft.PCR.Domain.UnitTests
{
	[Target("XUnitOutputTarget")]
	public class XUnitOutputTarget : TargetWithLayout
	{
		protected override void Write(LogEventInfo logEvent)
		{
			OutputHelper.WriteLine(RenderLogEvent(Layout, logEvent));
		}

		public static ITestOutputHelper OutputHelper { get; set; }
	}
}
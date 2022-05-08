
using Amusoft.PCR.Domain.UnitTests.Common;
using Xunit.Abstractions;

namespace Amusoft.PCR.Domain.UnitTests
{
	public class TestBase
	{
		public TestBase(ITestOutputHelper outputHelper)
		{
			XUnitOutputTarget.OutputHelper = outputHelper;
		}
	}
}
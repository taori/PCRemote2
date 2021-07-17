using System.Collections.Generic;
using System.Threading.Tasks;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.ProgramControl
{
	public class LaunchProgramFragment : ButtonListFragment
	{
		private readonly GrpcApplicationAgent _agent;

		public LaunchProgramFragment(GrpcApplicationAgent agent)
		{
			_agent = agent;
		}

		protected override ButtonListDataSource CreateDataSource()
		{
			return new ButtonListDataSource(GenerateData);
		}

		private Task<List<ButtonElement>> GenerateData()
		{
			return Task.FromResult(new List<ButtonElement>());
		}
	}
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public abstract class StaticButtonListFragment : ButtonListFragment
	{
		protected override ButtonListDataSource CreateDataSource()
		{
			return new ButtonListDataSource(Generate);
		}

		private Task<List<ButtonElement>> Generate()
		{
			return Task.FromResult(GetButtonElements().ToList());
		}

		protected abstract IEnumerable<ButtonElement> GetButtonElements();
	}
}
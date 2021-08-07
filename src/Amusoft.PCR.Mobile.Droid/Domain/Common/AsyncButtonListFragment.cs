using System.Collections.Generic;
using System.Threading.Tasks;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public abstract class AsyncButtonListFragment : ButtonListFragment
	{
		protected override ButtonListDataSource CreateDataSource()
		{
			return new ButtonListDataSource(GetButtonsAsync);
		}

		protected abstract Task<List<ButtonElement>> GetButtonsAsync();
	}
}
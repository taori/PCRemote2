using System.Collections.Generic;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public interface IComponentContainer
	{
		IList<IComponent> Components { get; }
	}
}
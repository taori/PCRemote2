using System;
using System.Linq;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public static class ComponentRunner
	{
		public static void Execute<TComponent>(IComponentContainer container, Action<TComponent> callback) where TComponent : IComponent
		{
			foreach (var component in container.Components.Cast<TComponent>().Where(d => d != null))
			{
				callback?.Invoke(component);
			}
		}
	}
}
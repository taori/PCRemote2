using System.Collections.Generic;
using System.Reflection;
using Amusoft.PCR.Blazor.Components;

namespace Amusoft.PCR.Server.Dependencies
{
	public class ClassLoader
	{
		public IEnumerable<Assembly> GetAssemblies()
		{
			yield return typeof(Tooltip).Assembly;
		}
	}
}
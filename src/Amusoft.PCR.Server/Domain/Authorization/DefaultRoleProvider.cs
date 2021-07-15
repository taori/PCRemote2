using System.Collections.Generic;
using Amusoft.PCR.Model.Statics;

namespace Amusoft.PCR.Server.Domain.Authorization
{
	public class DefaultRoleProvider : IRoleNameProvider
	{
		public IEnumerable<string> GetRoleNames()
		{
			yield return RoleNames.Administrator;
			yield return RoleNames.Functions;
			yield return RoleNames.Processes;
			yield return RoleNames.Computer;
			yield return RoleNames.Audio;
			yield return RoleNames.ActiveWindow;
		}
	}
}
using System;

namespace Amusoft.PCR.Server.Domain.Authorization
{
	public class JwtSettings
	{
		public string Issuer { get; set; }

		public string Key { get; set; }

		public TimeSpan TokenValidDuration { get; set; }
	}
}
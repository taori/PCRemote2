using System;

namespace Amusoft.PCR.Server.Domain.Authorization
{
	public class JwtSettings
	{
		public string Issuer { get; set; }

		public string Key { get; set; }

		public TimeSpan AccessTokenValidDuration { get; set; }

		public TimeSpan RefreshAccessTokenInterval { get; set; }

		public TimeSpan RefreshTokenValidDuration { get; set; }
	}
}
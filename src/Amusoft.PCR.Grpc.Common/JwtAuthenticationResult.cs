namespace Amusoft.PCR.Grpc.Common
{
	public class JwtAuthenticationResult
	{
		public bool AuthenticationRequired { get; set; }

		public string AccessToken { get; set; }

		public string RefreshToken { get; set; }
	}
}
namespace Amusoft.PCR.Grpc.Common
{
	public class JwtAuthenticationResult
	{
		public string AccessToken { get; set; }

		public string RefreshToken { get; set; }
	}
}
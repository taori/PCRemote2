namespace Amusoft.PCR.Server.BackgroundServices
{
	internal class DesktopIntegrationSettings
	{
		public enum PathCheckMode
		{
			Exact,
			FileCheckOnly
		}

		public string ExePath { get; set; }

		public PathCheckMode PathCheck { get; set; }
	}
}
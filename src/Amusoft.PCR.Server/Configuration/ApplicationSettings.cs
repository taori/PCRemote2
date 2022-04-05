using System;

namespace Amusoft.PCR.Server.Configuration;

public class ApplicationSettings
{
    public string ApplicationTitle { get; set; }
    public bool DropDatabaseOnStart { get; set; }
    public DesktopIntegrationSettings DesktopIntegration { get; set; }
    public ServerUrlTransmitterSettings ServerUrlTransmitter { get; set; }
    public AuthenticationSettings Authentication { get; set; }
    public JwtSettings Jwt { get; set; }
}

public class DesktopIntegrationSettings
{
    public string ExePath { get; set; }
}

public class ServerUrlTransmitterSettings
{
    public int Port { get; set; }
    public int[] PublicHttpsPorts { get; set; }
}

public class AuthenticationSettings
{
    public TimeSpan IdleResetThreshold { get; set; }
}

public class JwtSettings
{
    public TimeSpan RefreshAccessTokenInterval { get; set; }
    public TimeSpan RefreshTokenValidDuration { get; set; }
    public TimeSpan AccessTokenValidDuration { get; set; }
    public string Key { get; set; }
    public string Issuer { get; set; }
}

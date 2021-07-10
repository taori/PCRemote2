using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Model.Entities;
using Amusoft.PCR.Model.Statics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Amusoft.PCR.Server.Authorization
{
	public interface IJwtTokenService
	{
		Task<JwtAuthenticationResult> CreateAuthenticationAsync(string userName, string password);
		Task<JwtAuthenticationResult> RefreshAsync(string expiredAccessToken, string refreshToken);
		bool TryGetGetUserFromToken(string token, out string userName, out SecurityToken securityToken);
	}

	public class JwtTokenService : IJwtTokenService
	{
		private readonly TokenValidationParameters _tokenValidationParameters;
		private readonly ILogger<JwtTokenService> _logger;
		private readonly IOptions<JwtSettings> _options;
		private readonly UserManager<ApplicationUser> _userManager;

		public JwtTokenService(
			TokenValidationParameters tokenValidationParameters,
			ILogger<JwtTokenService> logger,
			IOptions<JwtSettings> options, 
			UserManager<ApplicationUser> userManager)
		{
			_tokenValidationParameters = tokenValidationParameters;
			_logger = logger;
			_options = options;
			_userManager = userManager;
		}

		public async Task<JwtAuthenticationResult> CreateAuthenticationAsync(string userName, string password)
		{
			_logger.LogInformation("Authenticating user {User}", userName);
			var user = await _userManager.FindByNameAsync(userName);
			if (!await _userManager.CheckPasswordAsync(user, password))
			{
				_logger.LogWarning("Failed login attempt for {User}", userName);
				return null;
			}

			return await CreateAuthenticationResultFromUserAsync(user);
		}

		private async Task<JwtAuthenticationResult> CreateAuthenticationResultFromUserAsync(ApplicationUser user)
		{
			var roles = await _userManager.GetRolesAsync(user);
			var handler = new JwtSecurityTokenHandler();
			var claims = new List<Claim>();
			claims.Add(new Claim(ClaimTypes.Name, user.UserName));
			claims.Add(new Claim(JwtRegisteredClaimNames.Aud, _tokenValidationParameters.ValidAudience));
			claims.AddRange(roles.Select(d => new Claim(ClaimTypes.Role, d)));

			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.Key));
			var securityToken = new JwtSecurityToken(
				_options.Value.Issuer,
				claims: claims,
				signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512),
				expires: DateTime.UtcNow.Add(_options.Value.TokenValidDuration));

			var outputToken = handler.WriteToken(securityToken);
			var refreshToken = GenerateRefreshToken();

			await _userManager.RemoveAuthenticationTokenAsync(user, JwtBearerDefaults.AuthenticationScheme, "RefreshToken");
			await _userManager.SetAuthenticationTokenAsync(user, JwtBearerDefaults.AuthenticationScheme, "RefreshToken",
				refreshToken);

			return new JwtAuthenticationResult()
			{
				AccessToken = outputToken,
				RefreshToken = refreshToken
			};
		}

		public async Task<JwtAuthenticationResult> RefreshAsync(string expiredAccessToken, string refreshToken)
		{
			_logger.LogTrace("Reading username from token {Token}", refreshToken);
			if (!TryGetGetUserFromToken(expiredAccessToken, out var userName, out var securityToken))
			{
				_logger.LogWarning("Failed to get user through token");
				return new JwtAuthenticationResult() { AuthenticationRequired = true };
			}

			_logger.LogTrace("Obtaining user through userName {@UserName}", userName);
			var user = await _userManager.FindByNameAsync(userName);
			if (user == null)
			{
				_logger.LogWarning("Failed to get user through principal");
				return new JwtAuthenticationResult() {AuthenticationRequired = true};
			}

			var presentRefreshToken = await _userManager.GetAuthenticationTokenAsync(user, JwtBearerDefaults.AuthenticationScheme, "RefreshToken");
			if (presentRefreshToken == null)
			{
				_logger.LogWarning("No refresh token available but there should be one");
				return new JwtAuthenticationResult() { AuthenticationRequired = true };
			}

			_logger.LogTrace("Comparing refresh tokens");
			if (refreshToken.Equals(presentRefreshToken, StringComparison.OrdinalIgnoreCase))
			{
				_logger.LogDebug("All checks passed, returning new authentication");
				return await CreateAuthenticationResultFromUserAsync(user);
			}
			
			return new JwtAuthenticationResult() { AuthenticationRequired = true };
		}

		public string GenerateRefreshToken(int length = 32)
		{
			var randomNumber = new byte[length];
			using (var generator = RandomNumberGenerator.Create())
			{
				generator.GetBytes(randomNumber);
				return Convert.ToBase64String(randomNumber);
			}
		}

		public bool TryGetGetUserFromToken(string token, out string userName, out SecurityToken securityToken)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var tokenValidationParameters = new TokenValidationParameters();
			tokenValidationParameters.ValidateIssuer = true;
			tokenValidationParameters.ValidateAudience = true;
			tokenValidationParameters.ValidateLifetime = false;
			tokenValidationParameters.ValidateIssuerSigningKey = true;
			tokenValidationParameters.ValidIssuer = _options.Value.Issuer;
			tokenValidationParameters.ValidAudience = _options.Value.Issuer;
			tokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.Key));
			var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

			userName = principal.Identity?.Name;
			if (userName == null)
				return false;

			return true;
		}
	}
}
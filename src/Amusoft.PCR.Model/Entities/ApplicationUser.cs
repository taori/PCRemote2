using System;
using Microsoft.AspNetCore.Identity;

namespace Amusoft.PCR.Model.Entities
{
	public enum UserType
	{
		User,
		Administrator,
	}

	public class ApplicationUser : IdentityUser
	{
		public UserType UserType { get; set; }
	}

	public class UserSettings
	{
		public Guid UserId { get; set; }

		public ApplicationUser User { get; set; }

		public TimeSpan MostRecentShutdownDelay { get; set; }
	}
}
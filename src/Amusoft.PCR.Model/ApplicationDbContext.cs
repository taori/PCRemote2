using Amusoft.PCR.Model.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Amusoft.PCR.Model
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<RefreshToken>()
				.HasKey(d => new { d.UserId, d.RefreshTokenId });

			builder.Entity<Permission>()
				.HasKey(d => new { d.UserId, d.SubjectId, d.PermissionType });
		}

		public DbSet<RefreshToken> RefreshTokens { get; set; }

		public DbSet<Permission> Permissions { get; set; }

		public DbSet<HostCommand> HostCommands { get; set; }

		public DbSet<AudioFeed> AudioFeeds { get; set; }

		public DbSet<AudioFeedAlias> AudioFeedAliases { get; set; }

		public DbSet<KeyValueSetting> KeyValueSettings { get; set; }
	}
}

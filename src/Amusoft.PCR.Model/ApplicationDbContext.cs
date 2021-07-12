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
		}

		public DbSet<RefreshToken> RefreshTokens { get; set; }
	}
}

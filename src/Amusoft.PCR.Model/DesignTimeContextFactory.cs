using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Amusoft.PCR.Model
{
	public class DesignTimeContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
	{
		public ApplicationDbContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
			optionsBuilder.UseSqlServer(
				"Server=(localdb)\\mssqllocaldb;Database=Amusoft.PCR.Server-Dev;Trusted_Connection=True;MultipleActiveResultSets=true");

			return new ApplicationDbContext(optionsBuilder.Options);
		}
	}
}
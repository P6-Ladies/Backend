using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace backend.Data
{
    public class PrototypeDbContextFactory : IDesignTimeDbContextFactory<PrototypeDbContext>
    {
        public PrototypeDbContext CreateDbContext(string[] args)
        {
            // 1. Build a config object from appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // 2. Read the connection string
            var connectionString = config.GetConnectionString("DbConnection");

            // 3. Build the DbContextOptions
            var builder = new DbContextOptionsBuilder<PrototypeDbContext>();
            builder.UseNpgsql(connectionString);

            // 4. Construct and return the context
            return new PrototypeDbContext(builder.Options);
        }
    }
}

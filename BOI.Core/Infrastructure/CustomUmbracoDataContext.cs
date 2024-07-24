using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BOI.Core.Infrastructure
{
    public class CustomUmbracoDataContext : DbContext
    {
        public CustomUmbracoDataContext(DbContextOptions<CustomUmbracoDataContext> options) : base(options)
        {
        }

        //public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
    }
}

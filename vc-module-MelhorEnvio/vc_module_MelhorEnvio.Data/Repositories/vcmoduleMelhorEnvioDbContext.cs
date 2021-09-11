using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;

namespace vc_module_MelhorEnvio.Data.Repositories
{
    public class vcmoduleMelhorEnvioDbContext : DbContextWithTriggers
    {
        public vcmoduleMelhorEnvioDbContext(DbContextOptions<vcmoduleMelhorEnvioDbContext> options)
          : base(options)
        {
        }

        protected vcmoduleMelhorEnvioDbContext(DbContextOptions options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.Entity<MyModuleEntity>().ToTable("MyModule").HasKey(x => x.Id);
            // modelBuilder.Entity<MyModuleEntity>().Property(x => x.Id).HasMaxLength(128);
            // base.OnModelCreating(modelBuilder);
        }
    }
}


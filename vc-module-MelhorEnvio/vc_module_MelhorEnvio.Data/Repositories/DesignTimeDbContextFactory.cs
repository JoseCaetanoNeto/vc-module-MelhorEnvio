using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace vc_module_MelhorEnvio.Data.Repositories
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<vcmoduleMelhorEnvioDbContext>
    {
        public vcmoduleMelhorEnvioDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<vcmoduleMelhorEnvioDbContext>();

            builder.UseSqlServer("Data Source=(local);Initial Catalog=VirtoCommerce3;Persist Security Info=True;User ID=virto;Password=virto;MultipleActiveResultSets=True;Connect Timeout=30");

            return new vcmoduleMelhorEnvioDbContext(builder.Options);
        }
    }
}

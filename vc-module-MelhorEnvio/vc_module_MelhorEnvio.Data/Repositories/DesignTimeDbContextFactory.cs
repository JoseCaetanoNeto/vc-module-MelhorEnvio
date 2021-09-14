using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace vc_module_MelhorEnvio.Data.Repositories
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ShipmentPackage2DbContext>
    {
        public ShipmentPackage2DbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ShipmentPackage2DbContext>();

            builder.UseSqlServer("Data Source=(local);Initial Catalog=VirtoCommerce3;Persist Security Info=True;User ID=virto;Password=virto;MultipleActiveResultSets=True;Connect Timeout=30");

            return new ShipmentPackage2DbContext(builder.Options);
        }
    }
}

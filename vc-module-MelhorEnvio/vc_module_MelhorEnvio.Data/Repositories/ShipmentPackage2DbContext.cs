using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
using vc_module_MelhorEnvio.Data.Model;
using VirtoCommerce.OrdersModule.Data.Repositories;

namespace vc_module_MelhorEnvio.Data.Repositories
{
    public class ShipmentPackage2DbContext : OrderDbContext
    {
        public ShipmentPackage2DbContext(DbContextOptions<ShipmentPackage2DbContext> builderOptions) : base(builderOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region ShipmentPackage2
            modelBuilder.Entity<ShipmentPackage2Entity>();
            #endregion

            base.OnModelCreating(modelBuilder);
        }
    }
}


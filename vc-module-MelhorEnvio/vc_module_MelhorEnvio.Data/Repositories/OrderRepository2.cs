using System.Linq;
using vc_module_MelhorEnvio.Data.Model;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Domain;

namespace vc_module_MelhorEnvio.Data.Repositories
{
    public class OrderRepository2 : OrderRepository
    {
        public OrderRepository2(ShipmentPackage2DbContext dbContext, IUnitOfWork unitOfWork = null) : base(dbContext, unitOfWork)
        {
        }

        public IQueryable<ShipmentPackage2Entity> ShipmentPackage2 => DbContext.Set<ShipmentPackage2Entity>();
    }
}

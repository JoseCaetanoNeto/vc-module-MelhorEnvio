using System.ComponentModel.DataAnnotations;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace vc_module_MelhorEnvio.Data.Model
{
    public class ShipmentPackage2Entity : ShipmentPackageEntity
    {
        public ShipmentPackage2Entity()
        {
            
        }

        [StringLength(64)]
        public string PackageState { get; set; }
        
        [StringLength(128)]
        public string TrackingCode { get; set; }
        
        [StringLength(128)]
        public string OuterId { get; set; }
        
        [StringLength(128)]
        public string Protocol { get; set; }
        

        public override ShipmentPackage ToModel(ShipmentPackage operation)
        {
            if (operation is ShipmentPackage2 order2)
            {
                order2.OuterId = OuterId;
                order2.TrackingCode = TrackingCode;
                order2.PackageState = PackageState;
                order2.Protocol = Protocol;
            }

            base.ToModel(operation);

            return operation;
        }

        public override ShipmentPackageEntity FromModel(ShipmentPackage operation, PrimaryKeyResolvingMap pkMap)
        {
            if (operation is ShipmentPackage2 order2)
            {
                OuterId = order2.OuterId;
                TrackingCode = order2.TrackingCode;
                PackageState = order2.PackageState;
                Protocol = order2.Protocol;
            }

            base.FromModel(operation, pkMap);

            return this;
        }

        public override void Patch(ShipmentPackageEntity operation)
        {
            if (operation is ShipmentPackage2Entity target)
            {
                target.OuterId = OuterId;
                target.TrackingCode = TrackingCode;
                target.PackageState = PackageState;
                target.Protocol = Protocol;
            }

            base.Patch(operation);
        }
    }
}

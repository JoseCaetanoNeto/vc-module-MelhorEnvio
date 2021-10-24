using System.Collections.Generic;
using VirtoCommerce.OrdersModule.Core.Model;

namespace vc_module_MelhorEnvio.Data.Model
{
    public class ShipmentPackage2 : ShipmentPackage
    {
        public ShipmentPackage2()
        {
        }
        
        [Auditable]
        public string OuterId { get; set; }

        [Auditable]
        public string PackageState { get; set; }

        [Auditable]
        public int? MinDays { get; set; }

        [Auditable]
        public int? MaxDays { get; set; }


        [Auditable]
        public string TrackingCode { get; set; }

        [Auditable]
        public string Protocol { get; set; }
    }
}

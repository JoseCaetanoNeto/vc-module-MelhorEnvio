using System.Collections.Generic;
using vc_module_MelhorEnvio.Core.Models;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model;

namespace vc_module_MelhorEnvio.Core
{
    public interface IMelhorEnvioService
    {
        CalculateOut Calculate(OptionCall pOptionCall, Store pStore, string pShipmentPostalCode, ICollection<LineItem> pItems, FulfillmentCenter fulfillmentCenter);
        CalculateOut Calculate(OptionCall pOptionCall, Store pStore, string pShipmentPostalCode, ICollection<VirtoCommerce.CartModule.Core.Model.LineItem> pItems, FulfillmentCenter fulfillmentCenter);
        Dictionary<ShipmentPackage, CartOut> SendMECart(OptionCall pOptionCall, OptionsSend pOptionsSend, CustomerOrder pCustomerOrder, Shipment pShipment, Store pStore, FulfillmentCenter pFulfillmentCenter);
        CheckoutOut CheckOut(OptionCall pOptionCall, string pOrder, Store pStore);
        GenerateOut Generate(OptionCall pOptionCall, string pOrder, Store pStore);
        PrintOut Print(OptionCall pOptionCall, PrintMode Modes, string pOrder, Store pStore);
        CancelOut CancelOrder(OptionCall pOptionCall, Store pStore, string pOrder, string pDescription);
        AgencieOut GetAgencyInfo(OptionCall pOptionCall, int pAgencyId, Store pStore);
        TrackingOut TrackingOrders(OptionCall pOptionCall, Store pStore, List<string> pOrders);
        List<string> GetFulfillmentCenters(List<string> ShipmentsWarehouseLocation, List<string> ItemsFulfillmentLocationCode, string MainFulfillmentCenterId);
        bool UpdatePackages(OptionCall pOptionCall, CustomerOrder pCustomerOrder);
    }
}
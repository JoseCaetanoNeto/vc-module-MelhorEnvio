using System;
using System.Linq;
using vc_module_MelhorEnvio.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Services;

namespace vc_module_MelhorEnvio.Core
{
    public class MelhorEnvioService2 : IMelhorEnvioService
    {
        public MelhorEnvioService2(ISettingsManager pSettingsManager, IStoreService pStoreService, IFulfillmentCenterService pFulfillmentCenterService, IMemberResolver pMemberResolver)
        {
            _settingsManager = pSettingsManager;
            _storeService = pStoreService;
            _fulfillmentCenterService = pFulfillmentCenterService;
            _memberResolver = pMemberResolver;
        }

        private readonly ISettingsManager _settingsManager;
        private readonly IStoreService _storeService;
        private readonly IFulfillmentCenterService _fulfillmentCenterService;
        private readonly IMemberResolver _memberResolver;

        public bool SendMECart(CustomerOrder pCustomerOrder)
        {
            var Items = pCustomerOrder.Items;
            if (Items.Count == 0)
                return false;

            var store = _storeService.GetByIdAsync(pCustomerOrder.StoreId).GetAwaiter().GetResult();
            var shipments = pCustomerOrder.Shipments.Where(s => s.ShipmentMethodCode == nameof(MelhorEnvioMethod));
            foreach (var shipment in shipments)
            {
                MelhorEnvioMethod melhorEnvioMethod = shipment.ShippingMethod as MelhorEnvioMethod;

                var FulfillmentCenterIds = melhorEnvioMethod.getFulfillmentCenters(
                    shipments.Select(s => s.FulfillmentCenterId).Where(s => s != null).Distinct().ToList(),
                    Items.Select(i => i.FulfillmentLocationCode).Where(s => s != null).Distinct().ToList(),
                    store.MainFulfillmentCenterId);

                var fulfillmentCenters = _fulfillmentCenterService.GetByIdsAsync(FulfillmentCenterIds).GetAwaiter().GetResult();

                var KeysValues = melhorEnvioMethod.SendMECart(pCustomerOrder, shipment, store, fulfillmentCenters.FirstOrDefault());
                foreach (var keyValue in KeysValues)
                {
                    var package = keyValue.Key as ShipmentPackage2;
                    var retMEApi = keyValue.Value;
                    if (retMEApi.errorOut != null)
                    {
                        string errors = retMEApi.errorOut.errors?.ToString();
                        if (errors != null)
                            throw new Exception($"{retMEApi.errorOut.message} - {errors}");
                        else
                            throw new Exception(retMEApi.errorOut.message);
                    }
                    package.OuterId = retMEApi.Id;
                    package.Protocol = retMEApi.Protocol;
                    shipment.Comment += $"PROTOCOL: {retMEApi.Protocol} {Environment.NewLine}";
                    if (retMEApi.AgencyId.HasValue)
                    {
                        var agency = melhorEnvioMethod.GetAgencyInfo(retMEApi.AgencyId.Value, store);
                        if (agency.errorOut == null || agency.errorOut.status_code == 0)
                        {
                            shipment.Comment += $"AGÊNCIA: {agency.Name} {Environment.NewLine}" +
                            $"{agency.CompanyName} {Environment.NewLine}" +
                            Environment.NewLine +

                            $"ENDEREÇO: {agency.address.address} " +
                            (string.IsNullOrWhiteSpace(agency.address.Number) ? string.Empty : $", Nº { agency.address.Number} ") +
                            (string.IsNullOrWhiteSpace(agency.address.Complement) ? string.Empty : $", {agency.address.Complement} ") +
                            (string.IsNullOrWhiteSpace(agency.address.District) ? string.Empty : $", {agency.address.District} ") +
                            (string.IsNullOrWhiteSpace(agency.address.City.city) ? string.Empty : $", {agency.address.City.city} ") +
                            (string.IsNullOrWhiteSpace(agency.address.City.State.StateAbbr) ? string.Empty : $" - {agency.address.City.State.StateAbbr} ") +

                            $"{Environment.NewLine}EMAIL: {agency.Email} {Environment.NewLine}" +
                            $"TELEFONE: {agency.phone.phone} {Environment.NewLine}{Environment.NewLine}";
                        }
                    }
                }

            }
            return true;
        }
    }

}

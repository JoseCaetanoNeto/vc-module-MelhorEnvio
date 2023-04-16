using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using vc_module_MelhorEnvio.Core.Model;
using vc_module_MelhorEnvio.Core.Models;
using VirtoCommerce.CartModule.Core.Model;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.ShippingModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model;
using static vc_module_MelhorEnvio.Core.ModuleConstants.Settings;
using OrderModel = VirtoCommerce.OrdersModule.Core.Model;

namespace vc_module_MelhorEnvio.Core
{
    public class MelhorEnvioMethod : ShippingMethod
    {

        public MelhorEnvioMethod(IMelhorEnvioService melhorEnvioService, ISettingsManager pSettingsManager, ICrudService<Store> pStoreService, IFulfillmentCenterService pFulfillmentCenterService, IDynamicPropertySearchService pDynamicPropertySearchService) : base(nameof(MelhorEnvioMethod))
        {
            _storeService = pStoreService;
            _fulfillmentCenterService = (ICrudService<FulfillmentCenter>)pFulfillmentCenterService;
            _melhorEnvioService = melhorEnvioService;
            _dynamicPropertySearchService = pDynamicPropertySearchService;
            _settingsManager = pSettingsManager;
        }

        private readonly ICrudService<Store> _storeService;
        private readonly ICrudService<FulfillmentCenter> _fulfillmentCenterService;
        private readonly IMelhorEnvioService _melhorEnvioService;
        private readonly IDynamicPropertySearchService _dynamicPropertySearchService;
        private readonly ISettingsManager _settingsManager;

        private bool Sandbox
        {
            get
            {
                bool.TryParse(Settings?.GetSettingValue(MelhorEnvio.Sandbox.Name,
                    MelhorEnvio.Sandbox.DefaultValue.ToString()), out bool cap);
                return cap;
            }
        }

        private string Token()
        {
            return Convert.ToString(_settingsManager.GetObjectSettings(MelhorEnvio.Token.Name, ModuleConstants.objectTypeRestrict, Id));
        }

        private string Client_id
        {
            get
            {
                return Settings?.GetSettingValue(MelhorEnvio.client_id.Name,
                    MelhorEnvio.client_id.DefaultValue.ToString());
            }
        }

        private string Client_secret
        {
            get
            {
                return Settings?.GetSettingValue(MelhorEnvio.client_secret.Name,
                    MelhorEnvio.client_secret.DefaultValue.ToString());
            }
        }

        private string Document
        {
            get
            {
                return Settings?.GetSettingValue(MelhorEnvio.Document.Name,
                    MelhorEnvio.Document.DefaultValue.ToString());
            }
        }

        private string StateRegister
        {
            get
            {
                return Settings?.GetSettingValue(MelhorEnvio.StateRegister.Name,
                    MelhorEnvio.StateRegister.DefaultValue.ToString());
            }
        }

        private string CompanyDocument
        {
            get
            {
                return Settings?.GetSettingValue(MelhorEnvio.CompanyDocument.Name,
                    MelhorEnvio.CompanyDocument.DefaultValue.ToString());
            }
        }

        private string EconomicActivityCode
        {
            get
            {
                return Settings?.GetSettingValue(MelhorEnvio.EconomicActivityCode.Name,
                    MelhorEnvio.EconomicActivityCode.DefaultValue.ToString());
            }
        }

        private bool NonCommercial
        {
            get
            {
                bool ret;
                bool.TryParse(Settings?.GetSettingValue(MelhorEnvio.NonCommercial.Name,
                    MelhorEnvio.NonCommercial.DefaultValue.ToString()), out ret);
                return ret;
            }
        }

        public string SendDataOnShippingStatus
        {
            get
            {
                return Settings?.GetSettingValue(MelhorEnvio.SendDataOnShippingStatus.Name,
                    MelhorEnvio.SendDataOnShippingStatus.DefaultValue.ToString());
            }
        }

        private int? AgencyJadLog
        {
            get
            {
                if (int.TryParse(Convert.ToString(Settings?.GetSettingValue(MelhorEnvio.AgencyJadLog.Name, MelhorEnvio.AgencyJadLog.DefaultValue)), out int intOut))
                    return intOut;
                return null;
            }
        }

        private int? AgencyAzul
        {
            get
            {
                if (int.TryParse(Convert.ToString(Settings?.GetSettingValue(MelhorEnvio.AgencyAzul.Name, MelhorEnvio.AgencyAzul.DefaultValue)), out int intOut))
                    return intOut;
                return null;
            }
        }

        private bool Checkout
        {
            get
            {
                return Settings.GetSettingValue(MelhorEnvio.Checkout.Name, (bool)MelhorEnvio.Checkout.DefaultValue);
            }
        }

        private OptionCall OptionCall()
        {
            return new OptionCall() { Client_id = Client_id, Client_secret = Client_secret, Sandbox = Sandbox, SaveToken = SaveToken, Token = Token };
        }


        public override IEnumerable<ShippingRate> CalculateRates(IEvaluationContext context)
        {
            //if (!(context is ShippingRateEvaluationContext shippingContext))
            if (!(context is ShippingEvaluationContext shippingContext))
            {
                throw new ArgumentException(nameof(context));
            }
            var retList = new List<ShippingRate>();

            if (shippingContext.ShoppingCart.Shipments == null || shippingContext.ShoppingCart.Shipments.Count == 0 || shippingContext.ShoppingCart.Shipments.FirstOrDefault().DeliveryAddress == null)
                return retList;

            var store = _storeService.GetByIdAsync(shippingContext.ShoppingCart.StoreId).Result;

            List<string> FulfillmentCenterIds = _melhorEnvioService.GetFulfillmentCenters(
                shippingContext.ShoppingCart.Shipments.Select(s => s.WarehouseLocation).Where(s => s != null).Distinct().ToList(),
                shippingContext.ShoppingCart.Items.Select(i => i.FulfillmentLocationCode).Where(s => s != null).Distinct().ToList(),
                store.MainFulfillmentCenterId);

            var fulfillmentCenters = _fulfillmentCenterService.GetAsync(FulfillmentCenterIds).Result;

            foreach (var fulfillmentCenter in fulfillmentCenters)
            {
                var ret = _melhorEnvioService.Calculate(OptionCall(), store, shippingContext.ShoppingCart.Shipments.FirstOrDefault().DeliveryAddress.PostalCode, shippingContext.ShoppingCart.Items, fulfillmentCenter);
                if (ret != null)
                {
                    foreach (var item in ret)
                    {
                        if (string.IsNullOrEmpty(item.Error))
                        {
                            var resultComp = new { NameService = item.Name, Company = item.company.Name, Picture = item.company.Picture, Id = item.Id, CustomDeliveryTime = item.CustomDeliveryTime, CustomDeliveryTimeMin = item.customDeliveryRange.Min, CustomDeliveryTimeMax = item.customDeliveryRange.Max };
                            retList.Add(new ShippingRate { Rate = item.CustomPrice, Currency = shippingContext.Currency, ShippingMethod = this, OptionName = BuildOptionName(item), OptionDescription = JsonConvert.SerializeObject(resultComp) });
                        }
                    }
                }
            }
            retList.Sort((o1, o2) => o1.Rate.CompareTo(o2.Rate));
            return retList;
        }


        public bool SendMECart(CustomerOrder pCustomerOrder)
        {
            var Items = pCustomerOrder.Items;
            if (Items.Count == 0)
                return false;

            var store = _storeService.GetByIdAsync(pCustomerOrder.StoreId).GetAwaiter().GetResult();
            var shipments = pCustomerOrder.Shipments.Where(s => s.ShipmentMethodCode == nameof(MelhorEnvioMethod));
            foreach (var shipment in shipments)
            {
                var FulfillmentCenterIds = _melhorEnvioService.GetFulfillmentCenters(
                    shipments.Select(s => s.FulfillmentCenterId).Where(s => s != null).Distinct().ToList(),
                    Items.Select(i => i.FulfillmentLocationCode).Where(s => s != null).Distinct().ToList(),
                    store.MainFulfillmentCenterId);

                var fulfillmentCenters = _fulfillmentCenterService.GetAsync(FulfillmentCenterIds).GetAwaiter().GetResult();

                OptionsSend pOptionsSend = new OptionsSend { Agency = GetAgency(shipment), CompanyDocument = CompanyDocument, Document = Document, EconomicActivityCode = EconomicActivityCode, NonCommercial = NonCommercial, StateRegister = StateRegister };
                var KeysValues = _melhorEnvioService.SendMECart(OptionCall(), pOptionsSend, pCustomerOrder, shipment, store, fulfillmentCenters.FirstOrDefault());
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
                        var agency = _melhorEnvioService.GetAgencyInfo(OptionCall(), retMEApi.AgencyId.Value, store);
                        if (agency.errorOut == null || agency.errorOut.status_code == 0)
                        {
                            shipment.Comment += $"AGÊNCIA: {agency.Name} {Environment.NewLine}" +
                            $"{agency.CompanyName} {Environment.NewLine}" +
                            Environment.NewLine +

                            $"ENDEREÇO: {agency.address.address} " +
                            (string.IsNullOrWhiteSpace(agency.address.Number) ? string.Empty : $", Nº {agency.address.Number} ") +
                            (string.IsNullOrWhiteSpace(agency.address.Complement) ? string.Empty : $", {agency.address.Complement} ") +
                            (string.IsNullOrWhiteSpace(agency.address.District) ? string.Empty : $", {agency.address.District} ") +
                            (string.IsNullOrWhiteSpace(agency.address.City.city) ? string.Empty : $", {agency.address.City.city} ") +
                            (string.IsNullOrWhiteSpace(agency.address.City.State.StateAbbr) ? string.Empty : $" - {agency.address.City.State.StateAbbr} ") +

                            $"{Environment.NewLine}EMAIL: {agency.Email} {Environment.NewLine}" +
                            $"TELEFONE: {agency.phone.phone} {Environment.NewLine}{Environment.NewLine}";
                        }
                    }
                    if (Checkout)
                    {
                        var resultCheckout = _melhorEnvioService.CheckOut(OptionCall(), package.OuterId, store);
                        shipment.Comment += $"Efetuado pagamento: {resultCheckout.purchase.Protocol} - {resultCheckout.purchase.Status} {Environment.NewLine}";

                        var resultGenerateList = _melhorEnvioService.Generate(OptionCall(), package.OuterId, store);
                        var resultGenerate = resultGenerateList[package.OuterId];
                        shipment.Comment += $"Etiqueta Gerada: {resultGenerate?.Status} - {resultGenerate?.Message} {Environment.NewLine}";

                        var LinkEtiquea = _melhorEnvioService.Print(OptionCall(), (PrintMode)1, package.OuterId, store);
                        shipment.Comment += $"Link Etiqueta: {LinkEtiquea?.Url} {Environment.NewLine}";

                        IList<DynamicProperty> dynamicProp = new List<DynamicProperty>();
                        dynamicProp.SetDynamicProp(_dynamicPropertySearchService, shipment, ModuleConstants.K_linkEtiqueta, LinkEtiquea?.Url).GetAwaiter().GetResult();
                    }
                }
            }
            return true;
        }

        public TrackingOut TrackingOrders(Store store, List<string> mE_Orders)
        {
            return _melhorEnvioService.TrackingOrders(OptionCall(), store, mE_Orders);
        }

        public CancelOut CancelOrder(Store pStore, string outerId, string pCancelReason)
        {
            return _melhorEnvioService.CancelOrder(OptionCall(), pStore, outerId, pCancelReason);
        }



        public bool UpdatePackages(CustomerOrder order)
        {
            return _melhorEnvioService.UpdatePackages(OptionCall(), order);
        }


        private void SaveToken(string newAccessToken)
        {
            _settingsManager.SeveObjectSettings(MelhorEnvio.Token.Name, ModuleConstants.objectTypeRestrict, Id, newAccessToken);
        }

        public static string BuildOptionName(CalculateOut.Item pItem)
        {
            return MelhorEnvioService.BuildOptionName(pItem);
        }

        public static ServiceOption DecodeOptionName(string pOptionName)
        {
            return MelhorEnvioService.DecodeOptionName(pOptionName);
        }

        private int? GetAgency(OrderModel.Shipment pShipment)
        {
            var Service = DecodeOptionName(pShipment.ShipmentMethodOption);
            switch (Service.CompanyID)
            {
                case ModuleConstants.K_Company_JADLOG:
                    return AgencyJadLog;
                case ModuleConstants.K_Company_AZULEXPRES:
                    return AgencyAzul;
                default:
                    return null;
            }
        }


    }
}
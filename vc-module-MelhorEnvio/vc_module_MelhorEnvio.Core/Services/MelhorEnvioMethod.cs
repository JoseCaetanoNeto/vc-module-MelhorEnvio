using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using vc_module_MelhorEnvio.Core.Model;
using vc_module_MelhorEnvio.Core.Models;
using VirtoCommerce.CartModule.Core.Model;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.ShippingModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model;
using OrderModel = VirtoCommerce.OrdersModule.Core.Model;

namespace vc_module_MelhorEnvio.Core
{
    public class MelhorEnvioMethod : ShippingMethod
    {
        private bool Sandbox
        {
            get
            {
                bool.TryParse(Settings?.GetSettingValue(ModuleConstants.Settings.MelhorEnvio.Sandbox.Name,
                    ModuleConstants.Settings.MelhorEnvio.Sandbox.DefaultValue.ToString()), out bool cap);
                return cap;
            }
        }

        private string Token()
        {
            return Convert.ToString(_settingsManager.GetObjectSettings(ModuleConstants.Settings.MelhorEnvio.Token.Name, ModuleConstants.objectTypeRestrict, Id));
        }

        private string Client_id
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.MelhorEnvio.client_id.Name,
                    ModuleConstants.Settings.MelhorEnvio.client_id.DefaultValue.ToString());
            }
        }

        private string Client_secret
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.MelhorEnvio.client_secret.Name,
                    ModuleConstants.Settings.MelhorEnvio.client_secret.DefaultValue.ToString());
            }
        }

        private string Document
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.MelhorEnvio.Document.Name,
                    ModuleConstants.Settings.MelhorEnvio.Document.DefaultValue.ToString());
            }
        }

        private string StateRegister
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.MelhorEnvio.StateRegister.Name,
                    ModuleConstants.Settings.MelhorEnvio.StateRegister.DefaultValue.ToString());
            }
        }

        private string CompanyDocument
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.MelhorEnvio.CompanyDocument.Name,
                    ModuleConstants.Settings.MelhorEnvio.CompanyDocument.DefaultValue.ToString());
            }
        }

        private string EconomicActivityCode
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.MelhorEnvio.EconomicActivityCode.Name,
                    ModuleConstants.Settings.MelhorEnvio.EconomicActivityCode.DefaultValue.ToString());
            }
        }

        private bool NonCommercial
        {
            get
            {
                bool ret;
                bool.TryParse(Settings?.GetSettingValue(ModuleConstants.Settings.MelhorEnvio.NonCommercial.Name,
                    ModuleConstants.Settings.MelhorEnvio.NonCommercial.DefaultValue.ToString()), out ret);
                return ret;
            }
        }

        public string SendDataOnShippingStatus
        {
            get
            {
                return Settings?.GetSettingValue(ModuleConstants.Settings.MelhorEnvio.SendDataOnShippingStatus.Name,
                    ModuleConstants.Settings.MelhorEnvio.SendDataOnShippingStatus.DefaultValue.ToString());
            }
        }

        public int? AgencyJadLog
        {
            get
            {
                if (int.TryParse(Convert.ToString(Settings?.GetSettingValue(ModuleConstants.Settings.MelhorEnvio.AgencyJadLog.Name, ModuleConstants.Settings.MelhorEnvio.AgencyJadLog.DefaultValue)), out int intOut))
                    return intOut;
                return null;
            }
        }

        public int? AgencyAzul
        {
            get
            {
                if (int.TryParse(Convert.ToString(Settings?.GetSettingValue(ModuleConstants.Settings.MelhorEnvio.AgencyAzul.Name, ModuleConstants.Settings.MelhorEnvio.AgencyAzul.DefaultValue)), out int intOut))
                    return intOut;
                return null;
            }
        }

        public MelhorEnvioMethod(ISettingsManager pSettingsManager, ICrudService<Store> pStoreService, IFulfillmentCenterService pFulfillmentCenterService, IMemberResolver pMemberResolver, IConversorStandardAddress pStandardAddress, IPlatformMemoryCache pPlatformMemoryCache) : base(nameof(MelhorEnvioMethod))
        {
            _settingsManager = pSettingsManager;
            _storeService = pStoreService;
            _fulfillmentCenterService = (ICrudService<FulfillmentCenter>)pFulfillmentCenterService;
            _memberResolver = pMemberResolver;
            _StandardAddress = pStandardAddress;
            _platformMemoryCache = pPlatformMemoryCache;
        }

        private readonly ISettingsManager _settingsManager;
        private readonly ICrudService<Store> _storeService;
        private readonly ICrudService<FulfillmentCenter> _fulfillmentCenterService;
        private readonly IMemberResolver _memberResolver;
        private readonly IConversorStandardAddress _StandardAddress;
        private readonly IPlatformMemoryCache _platformMemoryCache;

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

            List<string> FulfillmentCenterIds = getFulfillmentCenters(
                shippingContext.ShoppingCart.Shipments.Select(s => s.WarehouseLocation).Where(s => s != null).Distinct().ToList(),
                shippingContext.ShoppingCart.Items.Select(i => i.FulfillmentLocationCode).Where(s => s != null).Distinct().ToList(),
                store.MainFulfillmentCenterId);

            var fulfillmentCenters = _fulfillmentCenterService.GetAsync(FulfillmentCenterIds).Result;

            foreach (var fulfillmentCenter in fulfillmentCenters)
            {
                var ret = Calculate(store, shippingContext.ShoppingCart.Shipments.FirstOrDefault().DeliveryAddress.PostalCode, shippingContext.ShoppingCart.Items, fulfillmentCenter);
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

        public Models.CalculateOut Calculate(Store pStore, string pShipmentPostalCode, ICollection<OrderModel.LineItem> pItems, FulfillmentCenter fulfillmentCenter)
        {
            return CalculateInt(pStore, pShipmentPostalCode, fulfillmentCenter.Address.PostalCode, ToItems(pItems, fulfillmentCenter));
        }

        public Models.CalculateOut Calculate(Store pStore, string pShipmentPostalCode, ICollection<LineItem> pItems, FulfillmentCenter fulfillmentCenter)
        {
            return CalculateInt(pStore, pShipmentPostalCode, fulfillmentCenter.Address.PostalCode, ToItems(pItems, fulfillmentCenter));
        }

        public Dictionary<OrderModel.ShipmentPackage, Models.CartOut> SendMECart(OrderModel.CustomerOrder pCustomerOrder, OrderModel.Shipment pShipment, Store pStore, FulfillmentCenter pFulfillmentCenter)
        {
            string orderNumber = pCustomerOrder.Number;
            string customerId = pCustomerOrder.CustomerId;
            var invoiceKey = pCustomerOrder.DynamicProperties.FirstOrDefault(p => p.Name == ModuleConstants.K_InvoiceKey)?.Values.FirstOrDefault()?.Value;

            var customer = _memberResolver.ResolveMemberByIdAsync(customerId).GetAwaiter().GetResult() as Contact;
            var Service = DecodeOptionName(pShipment.ShipmentMethodOption);
            var stdAddressFulfillmentCenter = GetStandardAddress(pFulfillmentCenter.Address);
            var stdDeliveryAddress = GetStandardAddress(pShipment.DeliveryAddress);
            var cartIn = new Models.CartIn()
            {
                Service = Service.ServiceID,
                Agency = GetAgency(Service),
                from = new Models.CartIn.From()
                {
                    Name = pStore.Name,
                    Document = string.IsNullOrWhiteSpace(Document) ? null : Document, // CPF pessoa fisica
                    StateRegister = string.IsNullOrWhiteSpace(StateRegister) ? null : StateRegister, // inscrição estadual
                    CompanyDocument = string.IsNullOrWhiteSpace(CompanyDocument) ? null : CompanyDocument, // CNPJ
                    EconomicActivityCode = string.IsNullOrWhiteSpace(EconomicActivityCode) ? null : EconomicActivityCode, // CNAE do CNPJ
                    Email = pFulfillmentCenter.Address.Email,
                    Phone = pFulfillmentCenter.Address.Phone,
                    Address = stdAddressFulfillmentCenter?.Street ?? pFulfillmentCenter.Address.Line1,
                    Number = (stdAddressFulfillmentCenter != null && !stdAddressFulfillmentCenter.HouseNumberFallback) ? stdAddressFulfillmentCenter.Number : null,
                    Complement = stdAddressFulfillmentCenter?.Complement ?? pFulfillmentCenter.Address.Line2,
                    District = stdAddressFulfillmentCenter?.Neighborhood, // bairro 
                    City = pFulfillmentCenter.Address.City,
                    CountryId = CountryCode.ConvertThreeCodeToTwoCode(pFulfillmentCenter.Address.CountryCode),
                    PostalCode = pFulfillmentCenter.Address.PostalCode,
                    //Note = ""
                },
                to = new Models.CartIn.To()
                {
                    Name = string.Join(" ", pShipment.DeliveryAddress.FirstName, pShipment.DeliveryAddress.LastName),
                    Document = customer.TaxPayerId, // CPF pessoa fisica,
                    //StateRegister = "", // inscrição estadual
                    //CompanyDocument = "", // CNPJ, parametro
                    //EconomicActivityCode = // CNAE do CNPJ Para casos de envios reversos, deve ser utilizado o parâmetro to.economic_activity_code visto que o remetente passa a ser o próprio recebedor.
                    Email = pShipment.DeliveryAddress.Email,
                    Phone = pShipment.DeliveryAddress.Phone,
                    Address = (stdDeliveryAddress != null && stdDeliveryAddress.Number != null && !stdDeliveryAddress.HouseNumberFallback) ? stdDeliveryAddress.Street : pShipment.DeliveryAddress.Line1,
                    Number = (stdDeliveryAddress != null && stdDeliveryAddress.Number != null && !stdDeliveryAddress.HouseNumberFallback) ? stdDeliveryAddress.Number : null, // não tem separado, junto da linha 1
                    Complement = (stdDeliveryAddress != null && stdDeliveryAddress.Number != null && !stdDeliveryAddress.HouseNumberFallback) ? stdDeliveryAddress.Complement : pShipment.DeliveryAddress.Line2,
                    District = (stdDeliveryAddress != null && stdDeliveryAddress.Number != null && !stdDeliveryAddress.HouseNumberFallback) ? stdDeliveryAddress.Neighborhood : null, // bairro não tem
                    City = pShipment.DeliveryAddress.City,
                    StateAbbr = pShipment.DeliveryAddress.RegionId,
                    CountryId = CountryCode.ConvertThreeCodeToTwoCode(pShipment.DeliveryAddress.CountryCode),
                    PostalCode = pShipment.DeliveryAddress.PostalCode,
                    //Note = ""
                },
                options = new Models.CartIn.Options()
                {
                    InsuranceValue = pShipment.Items.Sum(i => i.LineItem.ExtendedPriceWithTax), // valor do seguro, deve conter o valor de seguro do envio, que deve corresponder ao valor dos itens/produtos enviados e deverá bater com o valor da NF.
                    Invoice = invoiceKey != null ? new Models.CartIn.Invoice() { Key = Convert.ToString(invoiceKey) } : null, // deve ser preenchida manualmente no paindel do mercado envio, antes de enviar
                    NonCommercial = NonCommercial, // indica se envio é não comercial
                    Platform = pStore.Name,// Nome da Plataforma
                    Tags = new List<Models.CartIn.Tag>() { { new Models.CartIn.Tag() { tag = orderNumber } } }
                },
                Products = new List<Models.CartIn.Product>(),
                Volumes = new List<Models.CartIn.Volume>(),
            };
            if (Service.CompanyID == ModuleConstants.K_Company_CORREIOS)
            {
                return SendCorreios(pShipment, pStore, cartIn);
            }
            else
            {
                return SendNotCorreios(pShipment, pStore, cartIn);
            }
        }

        private AddressStandardModel GetStandardAddress(VirtoCommerce.CoreModule.Core.Common.Address address)
        {
            if (!string.IsNullOrEmpty(address.Street))
            {
                return new AddressStandardModel()
                {
                    Street = address.Street,
                    Number = address.Number,
                    Complement = address.Line2,
                    Neighborhood = address.District,
                    City = address.City,
                    State = address.RegionId,
                    Country = address.CountryCode,
                    ZipCode = address.PostalCode,
                    HouseNumberFallback = false,
                };
            }

            return _StandardAddress.GetStandardAsync(address).GetAwaiter().GetResult();
        }

        public Models.TrackingOut TrackingOrders(Store pStore, List<string> pOrders)
        {
            var orders = new Models.TrackingIn() { Orders = pOrders };
            MelhorEnvioApi mes = new MelhorEnvioApi(Client_id, Client_secret, Sandbox, pStore.Name, pStore.AdminEmail, Token());
            mes.onSaveNewToken = SaveToken;
            var trackings = mes.Tracking(orders);
            return trackings;
        }

        public Models.CancelOut CancelOrder(Store pStore, string pOrder, string pDescription)
        {
            MelhorEnvioApi mes = new MelhorEnvioApi(Client_id, Client_secret, Sandbox, pStore.Name, pStore.AdminEmail, Token());
            mes.onSaveNewToken = SaveToken;
            return mes.Cancel(pOrder, pDescription);
        }

        public Models.AgencieOut GetAgencyInfo(int pAgencyId, Store pStore)
        {
            MelhorEnvioApi mes = new MelhorEnvioApi(Client_id, Client_secret, Sandbox, pStore.Name, pStore.AdminEmail, Token());
            mes.onSaveNewToken = SaveToken;
            return mes.GetAgencyInfo(pAgencyId);
        }

        public Models.CheckoutOut CheckOut(string pOrder, Store pStore)
        {
            MelhorEnvioApi mes = new MelhorEnvioApi(Client_id, Client_secret, Sandbox, pStore.Name, pStore.AdminEmail, Token());
            mes.onSaveNewToken = SaveToken;
            return mes.Checkout(new List<string>() { pOrder });
        }

        public Models.GenerateOut Generate(string pOrder, Store pStore)
        {
            MelhorEnvioApi mes = new MelhorEnvioApi(Client_id, Client_secret, Sandbox, pStore.Name, pStore.AdminEmail, Token());
            mes.onSaveNewToken = SaveToken;
            return mes.Generate(new List<string>() { pOrder });
        }

        public Models.PrintOut Print(PrintMode Modes, string pOrder,  Store pStore)
        {
            MelhorEnvioApi mes = new MelhorEnvioApi(Client_id, Client_secret, Sandbox, pStore.Name, pStore.AdminEmail, Token());
            mes.onSaveNewToken = SaveToken;
            return mes.Print(Modes, new List<string>() { pOrder });
        }

        public List<string> getFulfillmentCenters(List<string> ShipmentsWarehouseLocation, List<string> ItemsFulfillmentLocationCode, string MainFulfillmentCenterId)
        {
            var FulfillmentCenterIds = ShipmentsWarehouseLocation;

            if (FulfillmentCenterIds == null || FulfillmentCenterIds.Count == 0)
            {
                FulfillmentCenterIds = ItemsFulfillmentLocationCode;
            }

            if (FulfillmentCenterIds == null || FulfillmentCenterIds.Count == 0)
            {
                FulfillmentCenterIds = new List<string>() { MainFulfillmentCenterId };
            }

            return FulfillmentCenterIds;
        }

        private Dictionary<OrderModel.ShipmentPackage, Models.CartOut> SendCorreios(OrderModel.Shipment pShipment, Store pStore, Models.CartIn cartIn)
        {
            MelhorEnvioApi mes = new MelhorEnvioApi(Client_id, Client_secret, Sandbox, pStore.Name, pStore.AdminEmail, Token());
            mes.onSaveNewToken = SaveToken;
            var ret = new Dictionary<OrderModel.ShipmentPackage, Models.CartOut>(pShipment.Packages.Count);

            foreach (var Package in pShipment.Packages)
            {
                var Package2 = Package as ShipmentPackage2;
                if (string.IsNullOrEmpty(Package2.TrackingCode)) // caso já tenha o número de tack já foi enviado anteriormente
                {
                    cartIn.Volumes.Clear();
                    cartIn.Products.Clear();

                    cartIn.Volumes.Add(new Models.CartIn.Volume()
                    {
                        Height = Convert.ToInt32(Package.Height),
                        Length = Convert.ToInt32(Package.Length),
                        Weight = Convert.ToDouble(Package.Weight),
                        Width = Convert.ToInt32(Package.Width)
                    });

                    foreach (var item in Package.Items)
                    {
                        var lineItem = pShipment.Items.FirstOrDefault(i => i.LineItemId == item.LineItemId).LineItem;
                        cartIn.Products.Add(new Models.CartIn.Product()
                        {
                            Name = lineItem.Name, // item.LineItem is null, workaround
                            Quantity = item.Quantity,
                            UnitaryValue = lineItem.PriceWithTax - lineItem.DiscountAmount
                        });
                    }

                    var retMe = mes.InserirCart(cartIn);
                    ret.Add(Package, retMe);
                }
            }
            return ret;
        }

        private Dictionary<OrderModel.ShipmentPackage, Models.CartOut> SendNotCorreios(OrderModel.Shipment pShipment, Store pStore, Models.CartIn cartIn)
        {
            foreach (var item in pShipment.Items)
            {
                var lineItem = pShipment.Items.FirstOrDefault(i => i.LineItemId == item.LineItemId).LineItem;
                cartIn.Products.Add(new Models.CartIn.Product()
                {
                    Name = lineItem.Name, // item.LineItem is null, workaround
                    Quantity = item.Quantity,
                    UnitaryValue = lineItem.PriceWithTax - lineItem.DiscountAmount//  item.LineItem is null, workaround
                });
            }
            bool generateCall = false;
            foreach (var Package in pShipment.Packages)
            {
                var Package2 = Package as ShipmentPackage2;
                if (string.IsNullOrEmpty(Package2.TrackingCode)) if (string.IsNullOrEmpty(Package2.TrackingCode)) // caso já tenha o número de tack já foi enviado anteriormente
                        generateCall = true;

                cartIn.Volumes.Add(new Models.CartIn.Volume()
                {
                    Height = Convert.ToInt32(Package.Height),
                    Length = Convert.ToInt32(Package.Length),
                    Weight = Convert.ToDouble(Package.Weight),
                    Width = Convert.ToInt32(Package.Width)
                });
            }
            var ret = new Dictionary<OrderModel.ShipmentPackage, Models.CartOut>(pShipment.Packages.Count);
            if (generateCall)
            {
                MelhorEnvioApi mes = new MelhorEnvioApi(Client_id, Client_secret, Sandbox, pStore.Name, pStore.AdminEmail, Token());
                mes.onSaveNewToken = SaveToken;
                var retMe = mes.InserirCart(cartIn);
                foreach (var Package in pShipment.Packages)
                {
                    ret.Add(Package, retMe);
                }
            }
            return ret;
        }

        private Models.CalculateOut CalculateInt(Store pStore, string pShipmentPostalCode, string fulfillmentCenterPostalCode, List<Models.CalculateIn.Product> pItems)
        {
            var list = pItems.Select(o => CacheKey.With(o.Id, o.Quantity.ToString(), o.InsuranceValue.ToString())).Distinct().ToList();
            list.Sort();
            string key = CacheKey.With(GetType(), nameof(CalculateInt), pShipmentPostalCode, fulfillmentCenterPostalCode, string.Join('-', list));
            var result = _platformMemoryCache.GetOrCreateExclusive(key, (cacheEntry) =>
            {
                Models.CalculateIn calc = new Models.CalculateIn()
                {
                    from = new Models.CalculateIn.From()
                    {
                        PostalCode = fulfillmentCenterPostalCode
                    },
                    to = new Models.CalculateIn.To()
                    {
                        PostalCode = pShipmentPostalCode
                    },
                    Products = new List<Models.CalculateIn.Product>(),
                    options = new Models.CalculateIn.Options { OwnHand = false, Receipt = false },
                };

                calc.Products.AddRange(pItems);

                if (calc.Products.Count > 0)
                {
                    MelhorEnvioApi mes = new MelhorEnvioApi(Client_id, Client_secret, Sandbox, pStore.Name, pStore.AdminEmail, Token());
                    mes.onSaveNewToken = SaveToken;
                    var resultInt = mes.Calculate(calc);
                    cacheEntry.SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(10));
                    return resultInt;
                }
                return new Models.CalculateOut();
            });
            return result;
        }

        private List<Models.CalculateIn.Product> ToItems(ICollection<LineItem> pItems, FulfillmentCenter fulfillmentCenter)
        {
            List<Models.CalculateIn.Product> list = new List<Models.CalculateIn.Product>();
            foreach (var item in pItems)
            {
                if (item.FulfillmentCenterId == fulfillmentCenter.Id || string.IsNullOrEmpty(item.FulfillmentCenterId))
                {
                    list.Add(new Models.CalculateIn.Product()
                    {
                        Id = item.ProductId,
                        Weight = ConvertToKg(item.Weight, item.WeightUnit),
                        Height = ConvertToCm(item.Height, item.MeasureUnit),
                        Width = ConvertToCm(item.Width, item.MeasureUnit),
                        Length = ConvertToCm(item.Length, item.MeasureUnit),
                        Quantity = item.Quantity,
                        InsuranceValue = Convert.ToDouble(item.ListPrice - item.DiscountAmount),
                    });
                }
            }
            return list;
        }

        private List<Models.CalculateIn.Product> ToItems(ICollection<OrderModel.LineItem> pItems, FulfillmentCenter fulfillmentCenter)
        {
            List<Models.CalculateIn.Product> list = new List<Models.CalculateIn.Product>();
            foreach (var item in pItems)
            {
                if (item.FulfillmentCenterId == fulfillmentCenter.Id || string.IsNullOrEmpty(item.FulfillmentCenterId))
                {
                    list.Add(new Models.CalculateIn.Product()
                    {
                        Id = item.ProductId,
                        Weight = ConvertToKg(item.Weight, item.WeightUnit),
                        Height = ConvertToCm(item.Height, item.MeasureUnit),
                        Width = ConvertToCm(item.Width, item.MeasureUnit),
                        Length = ConvertToCm(item.Length, item.MeasureUnit),
                        Quantity = item.Quantity,
                        InsuranceValue = Convert.ToDouble(item.PlacedPrice),
                    });
                }
            }
            return list;
        }

        private double ConvertToKg(decimal? weight, string weightUnit)
        {
            if (!weight.HasValue)
                return 0;
            if (weightUnit.Equals("gram", StringComparison.InvariantCultureIgnoreCase))
                return Convert.ToDouble(weight.Value / 1000);
            else if (weightUnit.Equals("kg", StringComparison.InvariantCultureIgnoreCase))
                return Convert.ToDouble(weight.Value);
            else if (weightUnit.Equals("g", StringComparison.InvariantCultureIgnoreCase))
                return Convert.ToDouble(weight.Value / 1000);
            return Convert.ToDouble(weight.Value);
        }

        private int ConvertToCm(decimal? height, string measureUnit)
        {
            if (!height.HasValue)
                return 0;
            if (measureUnit.Equals("mm", StringComparison.InvariantCultureIgnoreCase))
                return Convert.ToInt32(height.Value / 1000);
            else if (measureUnit.Equals("cm", StringComparison.InvariantCultureIgnoreCase))
                return Convert.ToInt32(height.Value);
            else if (measureUnit.Equals("m", StringComparison.InvariantCultureIgnoreCase))
                return Convert.ToInt32(height.Value * 100);
            return Convert.ToInt32(height.Value);
        }

        private void SaveToken(string newAccessToken)
        {
            _settingsManager.SeveObjectSettings(ModuleConstants.Settings.MelhorEnvio.Token.Name, ModuleConstants.objectTypeRestrict, Id, newAccessToken);
        }

        public static string BuildOptionName(Models.CalculateOut.Item pItem)
        {
            return string.Join(" - ", string.Join(".", pItem.company.Id, pItem.Id), pItem.company.Name, pItem.Name);
        }

        public static ServiceOption DecodeOptionName(string pOptionName)
        {
            var array = pOptionName.Split(" - ");
            if (array.Length >= 3)
            {
                var code = array[0].Split(".");
                var ret = new ServiceOption(Convert.ToInt32(code[0]), Convert.ToInt32(code[1]), array[1], array[2]);
                return ret;
            }
            return new ServiceOption(0, 0, string.Empty, string.Empty);
        }

        private int? GetAgency(ServiceOption pService)
        {
            switch (pService.CompanyID)
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

    public class ServiceOption
    {
        public int ServiceID { get; }
        public string ServiceName { get; }
        public int CompanyID { get; }
        public string CompanyName { get; }

        public ServiceOption(int companyID, int serviceID, string companyName, string serviceName)
        {
            ServiceID = serviceID;
            ServiceName = serviceName;
            CompanyID = companyID;
            CompanyName = companyName;
        }

        public override bool Equals(object obj)
        {
            return obj is ServiceOption other &&
                   ServiceID == other.ServiceID &&
                   CompanyID == other.CompanyID &&
                   ServiceName == other.ServiceName &&
                   CompanyName == other.CompanyName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ServiceID, CompanyID, ServiceName, CompanyName);
        }
    }
}
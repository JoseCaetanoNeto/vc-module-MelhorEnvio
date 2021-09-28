using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CartModule.Core.Model;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.ShippingModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Services;
using Newtonsoft.Json;
using VirtoCommerce.CustomerModule.Core.Model;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Services;
using Microsoft.AspNetCore.Identity;
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

        public MelhorEnvioMethod(ISettingsManager settingsManager, IStoreService storeService, IFulfillmentCenterService fulfillmentCenterService, IMemberService memberService, UserManager<VirtoCommerce.Platform.Core.Security.ApplicationUser> userManager) : base(nameof(MelhorEnvioMethod))
        {
            _settingsManager = settingsManager;
            _storeService = storeService;
            _fulfillmentCenterService = fulfillmentCenterService;
            _memberService = memberService;
            _userManager = userManager;
        }

        private readonly ISettingsManager _settingsManager;
        private readonly IStoreService _storeService;
        private readonly IFulfillmentCenterService _fulfillmentCenterService;
        private readonly IMemberService _memberService;
        private readonly UserManager<VirtoCommerce.Platform.Core.Security.ApplicationUser> _userManager;

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

            var fulfillmentCenters = _fulfillmentCenterService.GetByIdsAsync(FulfillmentCenterIds).Result;

            foreach (var fulfillmentCenter in fulfillmentCenters)
            {
                var ret = Calculate(store, shippingContext.ShoppingCart.Shipments.FirstOrDefault().DeliveryAddress.PostalCode, shippingContext.ShoppingCart.Items, fulfillmentCenter);
                foreach (var item in ret)
                {
                    if (string.IsNullOrEmpty(item.Error))
                    {
                        var resultComp = new { NameService = item.Name, Company = item.company.Name, Picture = item.company.Picture, Id = item.Id, CustomDeliveryTime = item.CustomDeliveryTime, CustomDeliveryTimeMin = item.customDeliveryRange.Min, CustomDeliveryTimeMax = item.customDeliveryRange.Max };
                        retList.Add(new ShippingRate { Rate = item.CustomPrice, Currency = shippingContext.Currency, ShippingMethod = this, OptionName = BuildOptionName(item), OptionDescription = JsonConvert.SerializeObject(resultComp) });
                    }
                }

            }
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
            
            var customer = (Contact)GetCustomerAsync(customerId).GetAwaiter().GetResult();
            var Service = DecodeOptionName(pShipment.ShipmentMethodOption);
            var cartIn = new Models.CartIn()
            {
                Service = Service.ServiceID,
                from = new Models.CartIn.From()
                {
                    Name = pStore.Name,
                    Document = string.IsNullOrWhiteSpace(Document) ? null : Document, // CPF pessoa fisica
                    StateRegister = string.IsNullOrWhiteSpace(StateRegister) ? null : StateRegister, // inscrição estadual
                    CompanyDocument = string.IsNullOrWhiteSpace(CompanyDocument) ? null : CompanyDocument, // CNPJ
                    EconomicActivityCode = string.IsNullOrWhiteSpace(EconomicActivityCode) ? null : EconomicActivityCode, // CNAE do CNPJ
                    Email = pFulfillmentCenter.Address.Email,
                    Phone = pFulfillmentCenter.Address.Phone,
                    Address = pFulfillmentCenter.Address.Line1,
                    Number = pFulfillmentCenter.Address.Key,
                    Complement = pFulfillmentCenter.Address.Line2,
                    //District = pFulfillmentCenter.Address.RegionName, // bairro não tem
                    City = pFulfillmentCenter.Address.City,
                    CountryId = CountryCode.ConvertThreeCodeToTwoCode(pFulfillmentCenter.Address.CountryCode),
                    PostalCode = pFulfillmentCenter.Address.PostalCode,
                    //Note = ""
                },
                to = new Models.CartIn.To()
                {
                    Name = customer.Name,
                    Document = customer.TaxPayerId, // CPF pessoa fisica,
                    //StateRegister = "", // inscrição estadual
                    //CompanyDocument = "", // CNPJ, parametro
                    //EconomicActivityCode = // CNAE do CNPJ Para casos de envios reversos, deve ser utilizado o parâmetro to.economic_activity_code visto que o remetente passa a ser o próprio recebedor.
                    Email = pShipment.DeliveryAddress.Email,
                    Phone = pShipment.DeliveryAddress.Phone,
                    Address = pShipment.DeliveryAddress.Line1,
                    //Number = , // não tem separado, junto da linha 1
                    Complement = pShipment.DeliveryAddress.Line2,
                    //District = pShipment.DeliveryAddress., // bairro não tem
                    City = pShipment.DeliveryAddress.City,
                    StateAbbr = pShipment.DeliveryAddress.RegionId,
                    CountryId = CountryCode.ConvertThreeCodeToTwoCode(pShipment.DeliveryAddress.CountryCode),
                    PostalCode = pShipment.DeliveryAddress.PostalCode,
                    //Note = ""
                },
                options = new Models.CartIn.Options()
                {
                    InsuranceValue = pShipment.Items.Sum(i => i.LineItem.PriceWithTax), // valor do seguro, deve conter o valor de seguro do envio, que deve corresponder ao valor dos itens/produtos enviados e deverá bater com o valor da NF.
                    Invoice = invoiceKey != null? new Models.CartIn.Invoice() {  Key = Convert.ToString(invoiceKey) } : null, // deve ser preenchida manualmente no paindel do mercado envio, antes de enviar
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

        public Models.TrackingOut TrackingOrders(Store pStore, List<string> pOrders)
        {
            var orders = new Models.TrackingIn() { Orders = pOrders };
            MelhorEnvioService mes = new MelhorEnvioService(Client_id, Client_secret, Sandbox, pStore.Name, pStore.AdminEmail, Token());
            mes.onSaveNewToken = SaveToken;
            var trackings = mes.Tracking(orders);
            return trackings;
        }

        public Models.CancelOut CancelOrder(Store pStore, string pOrder, string pDescription)
        {
            MelhorEnvioService mes = new MelhorEnvioService(Client_id, Client_secret, Sandbox, pStore.Name, pStore.AdminEmail, Token());
            mes.onSaveNewToken = SaveToken;
            return mes.Cancel(pOrder, pDescription);
        }

        public Models.AgencieOut GetAgencyInfo(int pAgencyId, Store pStore)
        {
            MelhorEnvioService mes = new MelhorEnvioService(Client_id, Client_secret, Sandbox, pStore.Name, pStore.AdminEmail, Token());
            mes.onSaveNewToken = SaveToken;
            return mes.GetAgencyInfo(pAgencyId);
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
            MelhorEnvioService mes = new MelhorEnvioService(Client_id, Client_secret, Sandbox, pStore.Name, pStore.AdminEmail, Token());
            mes.onSaveNewToken = SaveToken;
            var ret = new Dictionary<OrderModel.ShipmentPackage, Models.CartOut>(pShipment.Packages.Count);

            foreach (var Package in pShipment.Packages)
            {
                cartIn.Volumes.Clear();
                cartIn.Volumes.Add(new Models.CartIn.Volume()
                {
                    Height = Convert.ToInt32(Package.Height),
                    Length = Convert.ToInt32(Package.Length),
                    Weight = Convert.ToInt32(Package.Weight),
                    Width = Convert.ToInt32(Package.Width)
                });

                foreach (var item in Package.Items)
                {
                    var lineItem = pShipment.Items.FirstOrDefault(i => i.LineItemId == item.LineItemId).LineItem;
                    cartIn.Products.Clear();
                    cartIn.Products.Add(new Models.CartIn.Product()
                    {
                        Name = lineItem.Name, // item.LineItem is null, workaround
                        Quantity = item.Quantity,
                        UnitaryValue = lineItem.PriceWithTax
                    });
                }

                var retMe = mes.InserirCart(cartIn);
                ret.Add(Package, retMe);
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
                    UnitaryValue = lineItem.PriceWithTax //  item.LineItem is null, workaround
                });
            }
            foreach (var Package in pShipment.Packages)
            {
                cartIn.Volumes.Add(new Models.CartIn.Volume()
                {
                    Height = Convert.ToInt32(Package.Height),
                    Length = Convert.ToInt32(Package.Length),
                    Weight = Convert.ToInt32(Package.Weight),
                    Width = Convert.ToInt32(Package.Width)
                });
            }
            MelhorEnvioService mes = new MelhorEnvioService(Client_id, Client_secret, Sandbox, pStore.Name, pStore.AdminEmail, Token());
            mes.onSaveNewToken = SaveToken;
            var retMe = mes.InserirCart(cartIn);
            var ret = new Dictionary<OrderModel.ShipmentPackage, Models.CartOut>(pShipment.Packages.Count);
            foreach (var Package in pShipment.Packages)
            {
                ret.Add(Package, retMe);
            }
            return ret;
        }

        private Models.CalculateOut CalculateInt(Store pStore, string pShipmentPostalCode, string fulfillmentCenterPostalCode, List<Models.CalculateIn.Product> pItems)
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
                MelhorEnvioService mes = new MelhorEnvioService(Client_id, Client_secret, Sandbox, pStore.Name, pStore.AdminEmail, Token());
                mes.onSaveNewToken = SaveToken;
                return mes.Calculate(calc);
            }
            return new Models.CalculateOut();
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
                        Id = item.Id,
                        Weight = ConvertToKg(item.Weight, item.WeightUnit),
                        Height = ConvertToCm(item.Height, item.MeasureUnit),
                        Width = ConvertToCm(item.Width, item.MeasureUnit),
                        Length = ConvertToCm(item.Length, item.MeasureUnit),
                        Quantity = item.Quantity,
                        InsuranceValue = Convert.ToDouble(item.SalePrice),
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
                        Id = item.Id,
                        Weight = ConvertToKg(item.Weight, item.WeightUnit),
                        Height = ConvertToCm(item.Height, item.MeasureUnit),
                        Width = ConvertToCm(item.Width, item.MeasureUnit),
                        Length = ConvertToCm(item.Length, item.MeasureUnit),
                        Quantity = item.Quantity,
                        InsuranceValue = Convert.ToDouble(item.PriceWithTax),
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
                Convert.ToDouble(weight.Value / 1000);
            else if (weightUnit.Equals("kg", StringComparison.InvariantCultureIgnoreCase))
                Convert.ToDouble(weight.Value);
            else if (weightUnit.Equals("g", StringComparison.InvariantCultureIgnoreCase))
                Convert.ToDouble(weight.Value / 1000);
            return Convert.ToDouble(weight.Value);
        }

        private int ConvertToCm(decimal? height, string measureUnit)
        {
            if (!height.HasValue)
                return 0;
            if (measureUnit.Equals("mm", StringComparison.InvariantCultureIgnoreCase))
                Convert.ToInt32(height.Value / 1000);
            else if (measureUnit.Equals("cm", StringComparison.InvariantCultureIgnoreCase))
                Convert.ToInt32(height.Value);
            else if (measureUnit.Equals("m", StringComparison.InvariantCultureIgnoreCase))
                Convert.ToInt32(height.Value * 100);
            return Convert.ToInt32(height.Value);
        }

        private void SaveToken(string newAccessToken)
        {
            _settingsManager.SeveObjectSettings(ModuleConstants.Settings.MelhorEnvio.Token.Name, ModuleConstants.objectTypeRestrict, Id, newAccessToken);
        }

        private async Task<Member> GetCustomerAsync(string customerId)
        {
            // Try to find contact
            var result = await _memberService.GetByIdAsync(customerId);

            if (result == null)
            {
                var user = await _userManager.FindByIdAsync(customerId);

                if (user != null)
                {
                    result = await _memberService.GetByIdAsync(user.MemberId);
                }
            }

            return result;
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

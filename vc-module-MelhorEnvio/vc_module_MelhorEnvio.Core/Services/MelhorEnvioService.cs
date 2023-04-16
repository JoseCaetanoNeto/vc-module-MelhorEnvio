using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using vc_module_MelhorEnvio.Core.Model;
using vc_module_MelhorEnvio.Core.Models;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.CartModule.Core.Model;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using OrderModel = VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model;

namespace vc_module_MelhorEnvio.Core
{
    public class MelhorEnvioService : IMelhorEnvioService
    {
        public MelhorEnvioService(IConversorStandardAddress pStandardAddress, IPlatformMemoryCache pPlatformMemoryCache, ICrudService<Store> pStoreService, IFulfillmentCenterService pFulfillmentCenterService, IMemberResolver pMemberResolver)
        {
            _storeService = pStoreService;
            _fulfillmentCenterService = (ICrudService<FulfillmentCenter>)pFulfillmentCenterService;
            _memberResolver = pMemberResolver;
            _StandardAddress = pStandardAddress;
            _platformMemoryCache = pPlatformMemoryCache;
        }

        private readonly ICrudService<Store> _storeService;
        private readonly ICrudService<FulfillmentCenter> _fulfillmentCenterService;
        private readonly IMemberResolver _memberResolver;
        private readonly IConversorStandardAddress _StandardAddress;
        private readonly IPlatformMemoryCache _platformMemoryCache;

        public bool UpdatePackages(OptionCall pOptionCall, OrderModel.CustomerOrder pCustomerOrder)
        {
            var shipments = pCustomerOrder.Shipments.Where(s => s.ShipmentMethodCode == nameof(MelhorEnvioMethod));
            var store = _storeService.GetByIdAsync(pCustomerOrder.StoreId).GetAwaiter().GetResult();
            var Items = pCustomerOrder.Items;
            foreach (var shipment in shipments)
            {
                var FulfillmentCenterIds = GetFulfillmentCenters(
                    shipments.Select(s => s.FulfillmentCenterId).Where(s => s != null).Distinct().ToList(),
                    Items.Select(i => i.FulfillmentLocationCode).Where(s => s != null).Distinct().ToList(),
                    store.MainFulfillmentCenterId);

                var fulfillmentCenters = _fulfillmentCenterService.GetAsync(FulfillmentCenterIds).GetAwaiter().GetResult();

                var shipmentSelect = Calculate(pOptionCall, store, pCustomerOrder.Shipments.FirstOrDefault().DeliveryAddress.PostalCode, pCustomerOrder.Items, fulfillmentCenters.FirstOrDefault()).FirstOrDefault(q => q.Id == DecodeOptionName(shipment.ShipmentMethodOption).ServiceID);
                if (shipmentSelect == null)
                    return false;

                shipment.Price = shipmentSelect.CustomPrice;
                if (shipment.Items == null)
                    shipment.Items = new List<OrderModel.ShipmentItem>();
                shipment.Items.Clear();
                shipment.Packages = new List<OrderModel.ShipmentPackage>();


                foreach (var Package in shipmentSelect.Packages)
                {
                    var shipPack = new ShipmentPackage2()
                    {
                        Height = Package.Dimensions.Height,
                        Length = Package.Dimensions.Length,
                        Width = Package.Dimensions.Width,
                        MeasureUnit = "cm",
                        Weight = Package.Weight,
                        WeightUnit = "kg",
                        PackageType = Package.Format,
                        MinDays = shipmentSelect.customDeliveryRange.Min,
                        MaxDays = shipmentSelect.customDeliveryRange.Max,
                        Items = new List<OrderModel.ShipmentItem>()
                    };
                    // removido, no insert não tem os ids e está dando erro
                    foreach (var product in Package.Products)
                    {
                        var LineItem = Items.FirstOrDefault(i => i.ProductId == product.Id);
                        var item = new OrderModel.ShipmentItem() { LineItemId = LineItem.Id, LineItem = LineItem, Quantity = product.Quantity };
                        shipPack.Items.Add(item);
                    }
                    shipment.Packages.Add(shipPack);
                }
            }
            return true;
        }

        public CalculateOut Calculate(OptionCall pOptionCall, Store pStore, string pShipmentPostalCode, ICollection<OrderModel.LineItem> pItems, FulfillmentCenter fulfillmentCenter)
        {
            return CalculateInt(pOptionCall, pStore, pShipmentPostalCode, fulfillmentCenter.Address.PostalCode, ToItems(pItems, fulfillmentCenter));
        }

        public CalculateOut Calculate(OptionCall pOptionCall, Store pStore, string pShipmentPostalCode, ICollection<LineItem> pItems, FulfillmentCenter fulfillmentCenter)
        {
            return CalculateInt(pOptionCall, pStore, pShipmentPostalCode, fulfillmentCenter.Address.PostalCode, ToItems(pItems, fulfillmentCenter));
        }

        public static string BuildOptionName(CalculateOut.Item pItem)
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

        public Dictionary<OrderModel.ShipmentPackage, CartOut> SendMECart(OptionCall pOptionCall, OptionsSend pOptionsSend, OrderModel.CustomerOrder pCustomerOrder, OrderModel.Shipment pShipment, Store pStore, FulfillmentCenter pFulfillmentCenter)
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
                Agency = pOptionsSend.Agency,
                from = new Models.CartIn.From()
                {
                    Name = pStore.Name,
                    Document = string.IsNullOrWhiteSpace(pOptionsSend.Document) ? null : pOptionsSend.Document, // CPF pessoa fisica
                    StateRegister = string.IsNullOrWhiteSpace(pOptionsSend.StateRegister) ? null : pOptionsSend.StateRegister, // inscrição estadual
                    CompanyDocument = string.IsNullOrWhiteSpace(pOptionsSend.CompanyDocument) ? null : pOptionsSend.CompanyDocument, // CNPJ
                    EconomicActivityCode = string.IsNullOrWhiteSpace(pOptionsSend.EconomicActivityCode) ? null : pOptionsSend.EconomicActivityCode, // CNAE do CNPJ
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
                options = new CartIn.Options()
                {
                    InsuranceValue = pShipment.Items.Sum(i => i.LineItem.ExtendedPriceWithTax), // valor do seguro, deve conter o valor de seguro do envio, que deve corresponder ao valor dos itens/produtos enviados e deverá bater com o valor da NF.
                    Invoice = invoiceKey != null ? new CartIn.Invoice() { Key = Convert.ToString(invoiceKey) } : null, // deve ser preenchida manualmente no paindel do mercado envio, antes de enviar
                    NonCommercial = pOptionsSend.NonCommercial, // indica se envio é não comercial
                    Platform = pStore.Name,// Nome da Plataforma
                    Tags = new List<CartIn.Tag>() { { new CartIn.Tag() { tag = orderNumber } } }
                },
                Products = new List<CartIn.Product>(),
                Volumes = new List<CartIn.Volume>(),
            };
            if (Service.CompanyID == ModuleConstants.K_Company_CORREIOS)
            {
                return SendCorreios(pOptionCall, pShipment, pStore, cartIn);
            }
            else
            {
                return SendNotCorreios(pOptionCall, pShipment, pStore, cartIn);
            }
        }

        public TrackingOut TrackingOrders(OptionCall pOptionCall, Store pStore, List<string> pOrders)
        {
            var orders = new TrackingIn() { Orders = pOrders };
            MelhorEnvioApi mes = new MelhorEnvioApi(pOptionCall.Client_id, pOptionCall.Client_secret, pOptionCall.Sandbox, pStore.Name, pStore.AdminEmail, pOptionCall.Token());
            mes.onSaveNewToken = pOptionCall.SaveToken;
            var trackings = mes.Tracking(orders);
            return trackings;
        }

        public CancelOut CancelOrder(OptionCall pOptionCall, Store pStore, string pOrder, string pDescription)
        {
            MelhorEnvioApi mes = new MelhorEnvioApi(pOptionCall.Client_id, pOptionCall.Client_secret, pOptionCall.Sandbox, pStore.Name, pStore.AdminEmail, pOptionCall.Token());
            mes.onSaveNewToken = pOptionCall.SaveToken;
            return mes.Cancel(pOrder, pDescription);
        }

        public AgencieOut GetAgencyInfo(OptionCall pOptionCall, int pAgencyId, Store pStore)
        {
            MelhorEnvioApi mes = new MelhorEnvioApi(pOptionCall.Client_id, pOptionCall.Client_secret, pOptionCall.Sandbox, pStore.Name, pStore.AdminEmail, pOptionCall.Token());
            mes.onSaveNewToken = pOptionCall.SaveToken;
            return mes.GetAgencyInfo(pAgencyId);
        }

        public CheckoutOut CheckOut(OptionCall pOptionCall, string pOrder, Store pStore)
        {
            MelhorEnvioApi mes = new MelhorEnvioApi(pOptionCall.Client_id, pOptionCall.Client_secret, pOptionCall.Sandbox, pStore.Name, pStore.AdminEmail, pOptionCall.Token());
            mes.onSaveNewToken = pOptionCall.SaveToken;
            return mes.Checkout(new List<string>() { pOrder });
        }

        public GenerateOut Generate(OptionCall pOptionCall, string pOrder, Store pStore)
        {
            MelhorEnvioApi mes = new MelhorEnvioApi(pOptionCall.Client_id, pOptionCall.Client_secret, pOptionCall.Sandbox, pStore.Name, pStore.AdminEmail, pOptionCall.Token());
            mes.onSaveNewToken = pOptionCall.SaveToken;
            return mes.Generate(new List<string>() { pOrder });
        }

        public PrintOut Print(OptionCall pOptionCall, PrintMode Modes, string pOrder, Store pStore)
        {
            MelhorEnvioApi mes = new MelhorEnvioApi(pOptionCall.Client_id, pOptionCall.Client_secret, pOptionCall.Sandbox, pStore.Name, pStore.AdminEmail, pOptionCall.Token());
            mes.onSaveNewToken = pOptionCall.SaveToken;
            return mes.Print(Modes, new List<string>() { pOrder });
        }

        public List<string> GetFulfillmentCenters(List<string> ShipmentsWarehouseLocation, List<string> ItemsFulfillmentLocationCode, string MainFulfillmentCenterId)
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

        private Dictionary<OrderModel.ShipmentPackage, CartOut> SendCorreios(OptionCall pOptionCall, OrderModel.Shipment pShipment, Store pStore, CartIn cartIn)
        {
            MelhorEnvioApi mes = new MelhorEnvioApi(pOptionCall.Client_id, pOptionCall.Client_secret, pOptionCall.Sandbox, pStore.Name, pStore.AdminEmail, pOptionCall.Token());
            mes.onSaveNewToken = pOptionCall.SaveToken;
            var ret = new Dictionary<OrderModel.ShipmentPackage, CartOut>(pShipment.Packages.Count);

            foreach (var Package in pShipment.Packages)
            {
                var Package2 = Package as ShipmentPackage2;
                if (string.IsNullOrEmpty(Package2.TrackingCode)) // caso já tenha o número de tack já foi enviado anteriormente
                {
                    cartIn.Volumes.Clear();
                    cartIn.Products.Clear();

                    cartIn.Volumes.Add(new CartIn.Volume()
                    {
                        Height = Convert.ToInt32(Package.Height),
                        Length = Convert.ToInt32(Package.Length),
                        Weight = Convert.ToDouble(Package.Weight),
                        Width = Convert.ToInt32(Package.Width)
                    });

                    foreach (var item in Package.Items)
                    {
                        var lineItem = pShipment.Items.FirstOrDefault(i => i.LineItemId == item.LineItemId).LineItem;
                        cartIn.Products.Add(new CartIn.Product()
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

        private Dictionary<OrderModel.ShipmentPackage, CartOut> SendNotCorreios(OptionCall pOptionCall, OrderModel.Shipment pShipment, Store pStore, CartIn cartIn)
        {
            foreach (var item in pShipment.Items)
            {
                var lineItem = pShipment.Items.FirstOrDefault(i => i.LineItemId == item.LineItemId).LineItem;
                cartIn.Products.Add(new CartIn.Product()
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

                cartIn.Volumes.Add(new CartIn.Volume()
                {
                    Height = Convert.ToInt32(Package.Height),
                    Length = Convert.ToInt32(Package.Length),
                    Weight = Convert.ToDouble(Package.Weight),
                    Width = Convert.ToInt32(Package.Width)
                });
            }
            var ret = new Dictionary<OrderModel.ShipmentPackage, CartOut>(pShipment.Packages.Count);
            if (generateCall)
            {
                MelhorEnvioApi mes = new MelhorEnvioApi(pOptionCall.Client_id, pOptionCall.Client_secret, pOptionCall.Sandbox, pStore.Name, pStore.AdminEmail, pOptionCall.Token());
                mes.onSaveNewToken = pOptionCall.SaveToken;
                var retMe = mes.InserirCart(cartIn);
                foreach (var Package in pShipment.Packages)
                {
                    ret.Add(Package, retMe);
                }
            }
            return ret;
        }

        private CalculateOut CalculateInt(OptionCall pOptionCall, Store pStore, string pShipmentPostalCode, string fulfillmentCenterPostalCode, List<CalculateIn.Product> pItems)
        {
            var list = pItems.Select(o => CacheKey.With(o.Id, o.Quantity.ToString(), o.InsuranceValue.ToString())).Distinct().ToList();
            list.Sort();
            string key = CacheKey.With(GetType(), nameof(CalculateInt), pShipmentPostalCode, fulfillmentCenterPostalCode, string.Join('-', list));
            var result = _platformMemoryCache.GetOrCreateExclusive(key, (cacheEntry) =>
            {
                CalculateIn calc = new CalculateIn()
                {
                    from = new CalculateIn.From()
                    {
                        PostalCode = fulfillmentCenterPostalCode
                    },
                    to = new CalculateIn.To()
                    {
                        PostalCode = pShipmentPostalCode
                    },
                    Products = new List<CalculateIn.Product>(),
                    options = new CalculateIn.Options { OwnHand = false, Receipt = false },
                };

                calc.Products.AddRange(pItems);

                if (calc.Products.Count > 0)
                {
                    MelhorEnvioApi mes = new MelhorEnvioApi(pOptionCall.Client_id, pOptionCall.Client_secret, pOptionCall.Sandbox, pStore.Name, pStore.AdminEmail, pOptionCall.Token());
                    mes.onSaveNewToken = pOptionCall.SaveToken;
                    var resultInt = mes.Calculate(calc);
                    cacheEntry.SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(10));
                    return resultInt;
                }
                return new CalculateOut();
            });
            return result;
        }

        private List<CalculateIn.Product> ToItems(ICollection<LineItem> pItems, FulfillmentCenter fulfillmentCenter)
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

        private List<CalculateIn.Product> ToItems(ICollection<OrderModel.LineItem> pItems, FulfillmentCenter fulfillmentCenter)
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
    }

    public class OptionsSend
    {
        public string Document { get; internal set; }
        public string StateRegister { get; internal set; }
        public string CompanyDocument { get; internal set; }
        public string EconomicActivityCode { get; internal set; }
        public bool NonCommercial { get; internal set; }
        public int? Agency { get; internal set; }
    }

    public class OptionCall
    {
        public Action<string> SaveToken;

        public Func<string> Token;
        public string Client_secret { get; set; }
        public bool Sandbox { get; set; }
        public string Client_id { get; set; }
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

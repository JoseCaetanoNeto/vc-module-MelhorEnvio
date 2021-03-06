using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vc_module_MelhorEnvio.Core;
using vc_module_MelhorEnvio.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.ShippingModule.Core.Model;
using VirtoCommerce.ShippingModule.Core.Model.Search;
using VirtoCommerce.ShippingModule.Core.Services;
using VirtoCommerce.StoreModule.Core.Model;

namespace vc_module_MelhorEnvio.Data.Handlers
{
    public class ProductInventoryChange
    {
        public string ProductId { get; set; }
        public int QuantityDelta { get; set; }
    }

    public class ActionJobArgument
    {
        public string CustomerOrderId { get; set; }
        public GenericChangedEntry<CustomerOrder> changedEntry { get; set; }
        public string TypeName { get; set; }
        public Func<CustomerOrder, bool> RunAction { get; set; }
    }


    /// <summary>
    /// Adjust inventory for ordered items 
    /// </summary>
    public class ShippmendOrderChangedEventHandler : IEventHandler<OrderChangedEvent>
    {
        private readonly ISettingsManager _settingsManager;
        private readonly ICrudService<Store> _storeService;
        private readonly ICrudService<CustomerOrder> _orderService;
        private readonly ICrudService<FulfillmentCenter> _fulfillmentCenterService;
        private readonly ISearchService<ShippingMethodsSearchCriteria, ShippingMethodsSearchResult, ShippingMethod> _shippingMethodsSearchService;
        private readonly IMelhorEnvioService _melhorEnvioService;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="inventoryService">Inventory service to use for adjusting inventories.</param>
        /// <param name="storeService">Implementation of store service.</param>
        /// <param name="settingsManager">Implementation of settings manager.</param>
        /// <param name="itemService">Implementation of item service</param>
        public ShippmendOrderChangedEventHandler(ICrudService<Store> storeService, ISettingsManager settingsManager, ICrudService<CustomerOrder> orderService, IFulfillmentCenterService fulfillmentCenterService, IShippingMethodsSearchService shippingMethodsSearchService, IMelhorEnvioService pMelhorEnvioService)
        {
            _settingsManager = settingsManager;
            _storeService = storeService;
            _orderService = orderService;
            _fulfillmentCenterService = (ICrudService<FulfillmentCenter>)fulfillmentCenterService;
            _shippingMethodsSearchService = (ISearchService<ShippingMethodsSearchCriteria, ShippingMethodsSearchResult, ShippingMethod>)shippingMethodsSearchService;
            _melhorEnvioService = pMelhorEnvioService;
        }

        public virtual Task Handle(OrderChangedEvent message)
        {
            foreach (var changedEntry in message.ChangedEntries)
            {
                var jobArguments = message.ChangedEntries.SelectMany(GetJobArgumentsForChangedEntry).ToArray();

                if (jobArguments.Any())
                {
                    return TryToProcessAsync(jobArguments);
                }
            }
            return Task.CompletedTask;
        }

        protected virtual ActionJobArgument[] GetJobArgumentsForChangedEntry(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var result = new List<ActionJobArgument>();

            if (!changedEntry.NewEntry.Shipments.Where(s => s.ShipmentMethodCode == nameof(MelhorEnvioMethod)).Any())
                return result.ToArray();

            var shippingMethodsSearchCriteria = AbstractTypeFactory<ShippingMethodsSearchCriteria>.TryCreateInstance();
            shippingMethodsSearchCriteria.StoreId = changedEntry.NewEntry.StoreId;
            shippingMethodsSearchCriteria.Codes = new[] { nameof(MelhorEnvioMethod) };
            shippingMethodsSearchCriteria.IsActive = true;
            var ShipmentMethods = _shippingMethodsSearchService.SearchAsync(shippingMethodsSearchCriteria).GetAwaiter().GetResult();
            var ShipmentMethod = ShipmentMethods.Results.FirstOrDefault(s => s.TypeName == nameof(MelhorEnvioMethod));
            string ShipmentMethod_Id = ShipmentMethod?.Id;

            if (IsShippingRecalc(changedEntry))
            {
                result.Add(new ActionJobArgument() { RunAction = UpdatePackages, changedEntry = changedEntry, TypeName = "OrderChanged", CustomerOrderId = changedEntry.NewEntry.Id });
            }
            
            if (IsOrderPaid(changedEntry))
            {
                result.Add(new ActionJobArgument() { changedEntry = changedEntry, TypeName = "OrderPaid", CustomerOrderId = changedEntry.NewEntry.Id });
            }

            if (IsSendDataShippingStatus(changedEntry, ShipmentMethod_Id))
            {
                result.Add(new ActionJobArgument() { RunAction = _melhorEnvioService.SendMECart, changedEntry = changedEntry, TypeName = "SendPackages", CustomerOrderId = changedEntry.NewEntry.Id });
            }

            return result.ToArray();
        }

        public virtual ProductInventoryChange[] GetProductChangesFor(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var oldLineItems = changedEntry.OldEntry.Items?.ToArray() ?? Array.Empty<LineItem>();
            var newLineItems = changedEntry.NewEntry.Items?.ToArray() ?? Array.Empty<LineItem>();

            var itemChanges = new List<ProductInventoryChange>();
            newLineItems.CompareTo(oldLineItems, EqualityComparer<LineItem>.Default, (state, changedItem, originalItem) =>
            {
                var newQuantity = changedItem.Quantity;
                var oldQuantity = originalItem.Quantity;

                if (changedEntry.EntryState == EntryState.Added || state == EntryState.Added)
                {
                    oldQuantity = 0;
                }
                else if (changedEntry.EntryState == EntryState.Deleted || state == EntryState.Deleted)
                {
                    newQuantity = 0;
                }

                if (oldQuantity != newQuantity)
                {
                    var itemChange = AbstractTypeFactory<ProductInventoryChange>.TryCreateInstance();
                    itemChange.ProductId = changedItem.ProductId;
                    itemChange.QuantityDelta = newQuantity - oldQuantity;
                    itemChanges.Add(itemChange);
                }
            });

            //Do not return unchanged records
            return itemChanges.Where(x => x.QuantityDelta != 0).ToArray();
        }

        public virtual async Task TryToProcessAsync(ActionJobArgument[] jobArguments)
        {
            var ordersByIdDict = (await _orderService.GetAsync(jobArguments.Select(x => x.CustomerOrderId).Distinct().ToList()))
                                .ToDictionary(x => x.Id)
                                .WithDefaultValue(null);

            var changedOrders = new List<CustomerOrder>();

            foreach (var jobArgument in jobArguments)
            {
                if (jobArgument.RunAction != null)
                {
                    var order = ordersByIdDict[jobArgument.CustomerOrderId];
                    if (order != null)
                    {
                        if (jobArgument.RunAction(order))
                        {
                            if (!changedOrders.Contains(order))
                            {
                                changedOrders.Add(order);
                            }
                        }
                    }
                }
            }

            if (changedOrders.Any())
            {
                await _orderService.SaveChangesAsync(changedOrders.ToArray());
            }

        }

        protected bool IsOrderPaid(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var oldPaidTotal = changedEntry.OldEntry.InPayments.Where(x => x.PaymentStatus == PaymentStatus.Paid).Sum(x => x.Sum);
            var newPaidTotal = changedEntry.NewEntry.InPayments.Where(x => x.PaymentStatus == PaymentStatus.Paid).Sum(x => x.Sum);
            return oldPaidTotal != newPaidTotal && changedEntry.NewEntry.Total <= newPaidTotal;
        }

        private bool IsShippingRecalc(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var productchanges = GetProductChangesFor(changedEntry);
            return productchanges.Any() || isChangedPostalCode(changedEntry);
        }

        private bool IsSendDataShippingStatus(GenericChangedEntry<CustomerOrder> pChangedEntry, string pShipmentMethod_Id)
        {
            string status = Convert.ToString(_settingsManager.GetObjectSettings(Core.ModuleConstants.Settings.MelhorEnvio.SendDataOnShippingStatus.Name, nameof(MelhorEnvioMethod), pShipmentMethod_Id));
            if (string.IsNullOrEmpty(status))
                return false;

            var oldStatusShipmentsCount = pChangedEntry.OldEntry.Shipments.Count(x => x.Status.EqualsInvariant(status));
            var newStatusShipmentsCount = pChangedEntry.NewEntry.Shipments.Count(x => x.Status.EqualsInvariant(status));
            return oldStatusShipmentsCount == 0 && newStatusShipmentsCount > 0;
        }

        private static bool isChangedPostalCode(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            return changedEntry.NewEntry.Shipments.FirstOrDefault()?.DeliveryAddress.PostalCode != changedEntry.OldEntry.Shipments.FirstOrDefault()?.DeliveryAddress.PostalCode;
        }

        private bool UpdatePackages(CustomerOrder pCustomerOrder)
        {
            var shipments = pCustomerOrder.Shipments.Where(s => s.ShipmentMethodCode == nameof(MelhorEnvioMethod));
            var store = _storeService.GetByIdAsync(pCustomerOrder.StoreId).GetAwaiter().GetResult();
            var Items = pCustomerOrder.Items;
            foreach (var shipment in shipments)
            {
                MelhorEnvioMethod melhorEnvioMethod = shipment.ShippingMethod as MelhorEnvioMethod;

                var FulfillmentCenterIds = melhorEnvioMethod.getFulfillmentCenters(
                    shipments.Select(s => s.FulfillmentCenterId).Where(s => s != null).Distinct().ToList(),
                    Items.Select(i => i.FulfillmentLocationCode).Where(s => s != null).Distinct().ToList(),
                    store.MainFulfillmentCenterId);

                var fulfillmentCenters = _fulfillmentCenterService.GetAsync(FulfillmentCenterIds).GetAwaiter().GetResult();

                var shipmentSelect = melhorEnvioMethod.Calculate(store, pCustomerOrder.Shipments.FirstOrDefault().DeliveryAddress.PostalCode, pCustomerOrder.Items, fulfillmentCenters.FirstOrDefault()).FirstOrDefault(q => q.Id == MelhorEnvioMethod.DecodeOptionName(shipment.ShipmentMethodOption).ServiceID);
                if (shipmentSelect == null)
                    return false;

                shipment.Price = shipmentSelect.CustomPrice;
                shipment.Items.Clear();
                shipment.Packages = new List<ShipmentPackage>();


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
                        Items = new List<ShipmentItem>()
                    };

                    foreach (var product in Package.Products)
                    {
                        var LineItem = Items.FirstOrDefault(i => i.ProductId == product.Id);
                        var item = new ShipmentItem() { LineItemId = LineItem.Id, LineItem = LineItem, Quantity = product.Quantity };
                        shipPack.Items.Add(item);
                    }
                    shipment.Packages.Add(shipPack);
                }
            }
            return true;
        }
        
    }
}

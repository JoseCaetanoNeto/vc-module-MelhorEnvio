using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vc_module_MelhorEnvio.Core;
using vc_module_MelhorEnvio.Data.Model;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.ShippingModule.Core.Model.Search;
using VirtoCommerce.ShippingModule.Core.Services;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace vc_module_MelhorEnvio.Data.Handlers
{
    


    /// <summary>
    /// Adjust inventory for ordered items 
    /// </summary>
    public class ShippmendCancelOrderEventHandler : IEventHandler<OrderChangedEvent>
    {
        private readonly IStoreService _storeService;
        private readonly ICustomerOrderService _orderService;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="inventoryService">Inventory service to use for adjusting inventories.</param>
        /// <param name="storeService">Implementation of store service.</param>
        /// <param name="settingsManager">Implementation of settings manager.</param>
        /// <param name="itemService">Implementation of item service</param>
        public ShippmendCancelOrderEventHandler(IStoreService storeService, ICustomerOrderService orderService)
        {
            _storeService = storeService;
            _orderService = orderService;
        }

        public virtual Task Handle(OrderChangedEvent message)
        {
            foreach (var changedEntry in message.ChangedEntries)
            {
                var jobArguments = message.ChangedEntries.SelectMany(GetJobArgumentsForChangedEntry).ToArray();

                if (jobArguments.Any())
                {
                    BackgroundJob.Enqueue(() => TryToCancelOrderSendsAsync(jobArguments));
                }
            }
            return Task.CompletedTask;
        }

        protected ShipmentToCancelJobArgument[] GetJobArgumentsForChangedEntry(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var toCancelShipments = new List<Shipment>();
            var isOrderCancelled = !changedEntry.OldEntry.IsCancelled && changedEntry.NewEntry.IsCancelled;
            if (isOrderCancelled)
            {
                toCancelShipments = changedEntry.NewEntry.Shipments?.Where(s => s.ShipmentMethodCode == nameof(MelhorEnvioMethod)).ToList();
            }
            else
            {
                foreach (var canceledShipment in changedEntry.NewEntry?.Shipments.Where(x => x.IsCancelled && x.ShipmentMethodCode == nameof(MelhorEnvioMethod)))
                {
                    var oldSamePayment = changedEntry.OldEntry?.Shipments.FirstOrDefault(x => x == canceledShipment);
                    if (oldSamePayment != null && !oldSamePayment.IsCancelled)
                    {
                        toCancelShipments.Add(canceledShipment);
                    }
                }
            }

            var shipmentToCancelJobArgument = new List<ShipmentToCancelJobArgument>();
            toCancelShipments.ForEach(s => {
                shipmentToCancelJobArgument.AddRange(s.Packages.Select(x => ShipmentToCancelJobArgument.FromChangedEntry(changedEntry, s, x as ShipmentPackage2)));
            });

            return shipmentToCancelJobArgument.ToArray();
        }

        public virtual async Task TryToCancelOrderSendsAsync(ShipmentToCancelJobArgument[] jobArguments)
        {
            var ordersByIdDict = (await _orderService.GetByIdsAsync(jobArguments.Select(x => x.CustomerOrderId).Distinct().ToArray()))
                                .ToDictionary(x => x.Id).WithDefaultValue(null);
            
            var storesByIdDict = (await _storeService.GetByIdsAsync(jobArguments.Select(x => x.StoreId).Distinct().ToArray()))
                                .ToDictionary(x => x.Id).WithDefaultValue(null);

            var changedOrders = new List<CustomerOrder>();
            foreach (var jobArgument in jobArguments)
            {
                var order = ordersByIdDict[jobArgument.CustomerOrderId];
                if (order != null)
                {
                    var shipmentToCancel = order.Shipments.FirstOrDefault(x => x.Id.EqualsInvariant(jobArgument.ShipmentId));
                    if (shipmentToCancel != null)
                    {
                        var package = shipmentToCancel.Packages.FirstOrDefault(x => x.Id.EqualsInvariant(jobArgument.PackageId));
                        CancelShipmentPackage(storesByIdDict[jobArgument.StoreId], shipmentToCancel, package as ShipmentPackage2, jobArgument.CancelReason);

                        if (!changedOrders.Contains(order))
                        {
                            changedOrders.Add(order);
                        }
                    }
                }
            }
            if (changedOrders.Any())
            {
                await _orderService.SaveChangesAsync(changedOrders.ToArray());
            }
        }

        private void CancelShipmentPackage(Store pStore, Shipment pShipment, ShipmentPackage2 pShipmentPackageToCancel, string pCancelReason)
        {
            var melhorEnvioMethod = pShipment.ShippingMethod as MelhorEnvioMethod;
            var ret = melhorEnvioMethod.CancelOrder(pStore, pShipmentPackageToCancel.OuterId, pCancelReason);
            if (ret != null)
            {
                if (ret.Canceled)
                {
                    if (!pShipment.CancelledDate.HasValue)
                        pShipment.CancelledDate = DateTime.Now;
                    pShipment.IsCancelled = true;
                    pShipment.Comment += $"{Environment.NewLine}PROTOCOL: {pShipmentPackageToCancel.Protocol} - CANCELAMENTO DO ENVIO FEITO COM SUCESSO {Environment.NewLine}";
                }
                else
                    pShipment.Comment += $"{Environment.NewLine}PROTOCOL: {pShipmentPackageToCancel.Protocol} - NÃO FOI POSSÍVEL CANCELAR - ITEM JÁ ENVIADO {Environment.NewLine}";
            }
        }
    }

    public class ShipmentToCancelJobArgument
    {
        public string CustomerOrderId { get; set; }
        public string ShipmentId { get; set; }
        public string PackageId { get; set; }
        public string OuterId { get; set; }
        public string StoreId { get; set; }
        public string CancelReason { get; set; }

        public static ShipmentToCancelJobArgument FromChangedEntry(GenericChangedEntry<CustomerOrder> changedEntry, Shipment pShipment, ShipmentPackage2 shipment)
        {
            var result = new ShipmentToCancelJobArgument
            {
                CustomerOrderId = changedEntry?.OldEntry.Id,
                PackageId = shipment?.Id,
                OuterId = shipment?.OuterId,
                ShipmentId = pShipment.Id,
                CancelReason = !string.IsNullOrWhiteSpace(pShipment?.CancelReason) ? pShipment?.CancelReason : !string.IsNullOrWhiteSpace(changedEntry?.NewEntry.CancelReason) ? changedEntry?.NewEntry.CancelReason : ModuleConstants.K_DefaultCancelReason,
                StoreId = changedEntry?.NewEntry.StoreId
            };
            return result;
        }
    }

}

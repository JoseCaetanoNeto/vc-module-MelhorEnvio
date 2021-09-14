using Hangfire;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using vc_module_MelhorEnvio.Core;
using vc_module_MelhorEnvio.Data.Model;
using vc_module_MelhorEnvio.Data.Repositories;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.ShippingModule.Core.Model.Search;
using VirtoCommerce.ShippingModule.Core.Services;
using VirtoCommerce.StoreModule.Core.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using VirtoCommerce.OrdersModule.Core.Model;

namespace vc_module_MelhorEnvio.Data.BackgroundJobs
{
    public class TrackingJob
    {
        private const string K_DeliveryPackageState = "Delivery";

        private const string K_NewStatus = "New";
        private const string k_SendStatus = "Send";
        private const string k_PaidStatus = "Paid";

        private const string k_ReadyToSendStatus = "ReadyToSend";

        private const string k_SentOrderStatus = "Sent";
        private const string k_ProcessingOrderStatus = "Processing";
        private const string k_PartiallySentOrderStatus = "PartiallySent";
        private const string k_ReadyToSendOrderStatus = "ReadyToSend";
        private const string K_CompletedOrderStatus = "Completed";

        private readonly ILogger _log;
        private readonly Func<IOrderRepository> _repositoryFactory;
        private readonly IShippingMethodsSearchService _shippingMethodsSearchService;
        private readonly ICustomerOrderService _customerOrderService;
        private readonly IStoreService _storeService;
        private readonly ISettingsManager _settingsManager;

        public TrackingJob(ISettingsManager settingsManager, IStoreService storeService, ICustomerOrderService customerOrderService, ILogger<TrackingJob> log, Func<IOrderRepository> repositoryFactory, IShippingMethodsSearchService shippingMethodsSearchService)
        {
            _settingsManager = settingsManager;
            _storeService = storeService;
            _customerOrderService = customerOrderService;
            _log = log;
            _repositoryFactory = repositoryFactory;
            _shippingMethodsSearchService = shippingMethodsSearchService;
        }

        [DisableConcurrentExecution(10)]
        public async Task Process()
        {
            _log.LogTrace($"Start processing TrackingJob job");

            var shippingMethodsSearchCriteria = AbstractTypeFactory<ShippingMethodsSearchCriteria>.TryCreateInstance();
            shippingMethodsSearchCriteria.Codes = new[] { nameof(MelhorEnvioMethod) };
            shippingMethodsSearchCriteria.IsActive = true;
            var authorizePaymentMethods = await _shippingMethodsSearchService.SearchShippingMethodsAsync(shippingMethodsSearchCriteria);

            // está trazendo itens inativos, mesmo indicando no filtro que é só ativo
            var activePaymentMethods = authorizePaymentMethods.Results.Where(p => p.IsActive).ToList();

            using (var repository = _repositoryFactory())
            {
                foreach (var activePaymentMethod in activePaymentMethods)
                {
                    //repository.DisableChangesTracking();
                    //string status = Convert.ToString(_settingsManager.GetObjectSettings(ModuleConstants.Settings.MelhorEnvio.StateRegister.Name, nameof(MelhorEnvioMethod), activePaymentMethod.Id));
                    //if (status == k_SendStatus)
                    //continue;
                    var query = ((OrderRepository2)repository).ShipmentPackage2;
                    query = query.Where(
                        s =>
                        s.Shipment.CustomerOrder.StoreId == activePaymentMethod.StoreId
                        /* && s.Shipment.Status == status */
                        && s.Shipment.ShipmentMethodCode == nameof(MelhorEnvioMethod)
                        && (s.OuterId != string.Empty && s.OuterId != null)
                        && (s.PackageState != K_DeliveryPackageState));

                    var queryPackages = await query.Select(p => new
                    {
                        CustomerOrderId = p.Shipment.CustomerOrderId,
                        ShipmentId = p.ShipmentId,
                        PackageId = p.Id,
                        OuterId = p.OuterId
                    }).ToArrayAsync();

                    List<CustomerOrder> customerOrderToUpdate = new List<CustomerOrder>();

                    if (queryPackages.Length > 0)
                    {
                        var store = await _storeService.GetByIdAsync(activePaymentMethod.StoreId);
                        var arryCustomerOrdes = queryPackages.Select(p => p.CustomerOrderId).ToArray();
                        var CustomerOrdes = await _customerOrderService.GetByIdsAsync(arryCustomerOrdes);
                        var shippingMethod = CustomerOrdes.FirstOrDefault().Shipments.FirstOrDefault(s => s.ShipmentMethodCode == nameof(MelhorEnvioMethod)).ShippingMethod as MelhorEnvioMethod;

                        var ME_Orders = queryPackages.Select(p => p.OuterId).ToList();

                        var resultTracking = shippingMethod.TrackingOrders(store, ME_Orders);

                        // update changes
                        foreach (var queryPackage in queryPackages)
                        {
                            var CustomerOrde = CustomerOrdes.FirstOrDefault(c => c.Id == queryPackage.CustomerOrderId);
                            var Shipment = CustomerOrde.Shipments.FirstOrDefault(s => s.Id == queryPackage.ShipmentId);
                            var Package = Shipment.Packages.FirstOrDefault(p => p.Id == queryPackage.PackageId) as ShipmentPackage2;

                            // item cancelado, volta status
                            if (!resultTracking.ContainsKey(queryPackage.OuterId))
                            {
                                CancelShipmentPackage(Shipment, Package);
                                if (!customerOrderToUpdate.Contains(CustomerOrde)) customerOrderToUpdate.Add(CustomerOrde);

                                continue;
                            }

                            var tracking = resultTracking[queryPackage.OuterId];

                            if ((tracking.Tracking ?? string.Empty) != (Package.TrackingCode ?? string.Empty))
                            {
                                Shipment.Comment += $"PROTOCOL: {Package.Protocol} ->TRACK: {tracking.Tracking} {Environment.NewLine}";
                                Package.TrackingCode = tracking.Tracking;
                                if (!customerOrderToUpdate.Contains(CustomerOrde)) customerOrderToUpdate.Add(CustomerOrde);
                            }

                            if (tracking.ExpiredAt.HasValue)
                            {
                                CancelShipmentPackage(Shipment, Package);
                                if (!customerOrderToUpdate.Contains(CustomerOrde)) customerOrderToUpdate.Add(CustomerOrde);
                            }
                            else if (tracking.CanceledAt.HasValue)
                            {
                                CancelShipmentPackage(Shipment, Package);
                                if (!customerOrderToUpdate.Contains(CustomerOrde)) customerOrderToUpdate.Add(CustomerOrde);
                            }
                            else if (tracking.DeliveredAt.HasValue && Package.PackageState != K_DeliveryPackageState)
                            {
                                Package.PackageState = K_DeliveryPackageState;
                                if (Shipment.Packages.All(s => (s as ShipmentPackage2).PackageState == K_DeliveryPackageState))
                                {
                                    CustomerOrde.Status = K_CompletedOrderStatus;
                                }
                                if (!customerOrderToUpdate.Contains(CustomerOrde)) customerOrderToUpdate.Add(CustomerOrde);
                            }
                            else if (tracking.PostedAt.HasValue && Package.PackageState != k_SendStatus)
                            {
                                Package.PackageState = k_SendStatus;
                                if (Shipment.Packages.All(s => (s as ShipmentPackage2).PackageState == k_SendStatus))
                                {
                                    CustomerOrde.Status = k_SentOrderStatus;
                                    Shipment.Status = k_SendStatus;
                                }
                                else
                                {
                                    CustomerOrde.Status = k_PartiallySentOrderStatus;
                                }
                                if (!customerOrderToUpdate.Contains(CustomerOrde)) customerOrderToUpdate.Add(CustomerOrde);
                            }
                            else if (tracking.PaidAt.HasValue && string.IsNullOrWhiteSpace(Package.PackageState))
                            {
                                var list = new List<string>() { { k_PaidStatus } };
                                Package.PackageState = k_PaidStatus;
                                if (CustomerOrde.Status == k_ProcessingOrderStatus && Shipment.Packages.All(s => list.Contains((s as ShipmentPackage2).PackageState)))
                                {
                                    CustomerOrde.Status = k_ReadyToSendOrderStatus;
                                }
                                if (!customerOrderToUpdate.Contains(CustomerOrde)) customerOrderToUpdate.Add(CustomerOrde);
                            }

                        }
                        // save changes
                        if (customerOrderToUpdate.Count > 0)
                            await _customerOrderService.SaveChangesAsync(customerOrderToUpdate.ToArray());
                    }
                }
            }

            _log.LogTrace($"Complete processing TrackingJob job");

            static void CancelShipmentPackage(Shipment Shipment, ShipmentPackage2 Package)
            {
                Shipment.Comment += $"{Environment.NewLine}PROTOCOL: {Package.Protocol} - CANCELED {Environment.NewLine}{Environment.NewLine}";
                Shipment.Status = K_NewStatus;
                Package.TrackingCode = string.Empty;
                Package.OuterId = string.Empty;
            }
        }
    }
}

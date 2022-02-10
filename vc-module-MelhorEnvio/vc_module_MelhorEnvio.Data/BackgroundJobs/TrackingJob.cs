using Hangfire;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using vc_module_MelhorEnvio.Core;
using vc_module_MelhorEnvio.Core.Model;
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
using VirtoCommerce.NotificationsModule.Core.Services;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Notifications;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Security;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.NotificationsModule.Core.Extensions;
using VirtoCommerce.NotificationsModule.Core.Model;
using vc_module_MelhorEnvio.Core.Notifications;

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
        private readonly INotificationSearchService _notificationSearchService;
        private readonly INotificationSender _notificationSender;
        private readonly IMemberResolver _memberResolver;

        public TrackingJob(ISettingsManager settingsManager, IStoreService storeService, ICustomerOrderService customerOrderService, ILogger<TrackingJob> log, Func<IOrderRepository> repositoryFactory, IShippingMethodsSearchService shippingMethodsSearchService, INotificationSearchService notificationSearchService, INotificationSender notificationSender, IMemberResolver pMemberResolver)
        {
            _settingsManager = settingsManager;
            _storeService = storeService;
            _customerOrderService = customerOrderService;
            _log = log;
            _repositoryFactory = repositoryFactory;
            _shippingMethodsSearchService = shippingMethodsSearchService;
            _notificationSearchService = notificationSearchService;
            _notificationSender = notificationSender;
            _memberResolver = pMemberResolver;
        }

        [DisableConcurrentExecution(10)]
        public async Task Process()
        {
            _log.LogTrace($"Start processing TrackingJob job");

            var shippingMethodsSearchCriteria = AbstractTypeFactory<ShippingMethodsSearchCriteria>.TryCreateInstance();
            shippingMethodsSearchCriteria.Codes = new[] { nameof(MelhorEnvioMethod) };
            shippingMethodsSearchCriteria.IsActive = true;
            var authorizePaymentMethods = await _shippingMethodsSearchService.SearchShippingMethodsAsync(shippingMethodsSearchCriteria);

            // est� trazendo itens inativos, mesmo indicando no filtro que � s� ativo
            var activePaymentMethods = authorizePaymentMethods.Results.Where(p => p.IsActive).ToList();

            using (var repository = _repositoryFactory())
            {
                foreach (var activePaymentMethod in activePaymentMethods)
                {
                    string status = Convert.ToString(_settingsManager.GetObjectSettings(ModuleConstants.Settings.MelhorEnvio.SendDataOnShippingStatus.Name, nameof(MelhorEnvioMethod), activePaymentMethod.Id));
                    var query = ((OrderRepository2)repository).ShipmentPackage2;
                    query = query.Where(
                        p =>
                        p.Shipment.CustomerOrder.StoreId == activePaymentMethod.StoreId
                        && p.Shipment.ShipmentMethodCode == nameof(MelhorEnvioMethod)
                        // caso exista OuterID avalair status do pacote, caso n�o exista avalia status do Envio como todo, para contemplar situa��o onde colocou no status e n�o gerou identificado por cr�tica na api ou falha de comunica��o
                        && ((p.OuterId != string.Empty && p.OuterId != null && p.PackageState != K_DeliveryPackageState) || (p.OuterId == null && p.Shipment.Status == status)));

                    var queryPackages = await query.Select(p => new
                    {
                        CustomerOrderId = p.Shipment.CustomerOrderId,
                        ShipmentId = p.ShipmentId,
                        PackageId = p.Id,
                        CustomerId = p.Shipment.CustomerOrder.CustomerId,
                        OuterId = p.OuterId
                    }).ToArrayAsync();

                    List<CustomerOrder> customerOrderToUpdate = new List<CustomerOrder>();

                    if (queryPackages.Length > 0)
                    {
                        var store = await _storeService.GetByIdAsync(activePaymentMethod.StoreId);
                        var arryCustomerOrdes = queryPackages.Select(p => p.CustomerOrderId).ToArray();
                        var CustomerOrdes = await _customerOrderService.GetByIdsAsync(arryCustomerOrdes);
                        var shippingMethod = CustomerOrdes.FirstOrDefault().Shipments.FirstOrDefault(s => s.ShipmentMethodCode == nameof(MelhorEnvioMethod)).ShippingMethod as MelhorEnvioMethod;

                        var ME_Orders = queryPackages.Select(p => p.OuterId).Where(i => !string.IsNullOrEmpty(i)).ToList();

                        var resultTracking = shippingMethod.TrackingOrders(store, ME_Orders);

                        if (resultTracking.errorOut != null && resultTracking.errorOut.status_code != 0)
                        {
                            _log.LogTrace($"Error - resultTracking: {resultTracking.errorOut.message}");
                            continue;
                        }

                        // update changes
                        foreach (var queryPackage in queryPackages)
                        {
                            var CustomerOrde = CustomerOrdes.FirstOrDefault(c => c.Id == queryPackage.CustomerOrderId);
                            var Shipment = CustomerOrde.Shipments.FirstOrDefault(s => s.Id == queryPackage.ShipmentId);
                            var Package = Shipment.Packages.FirstOrDefault(p => p.Id == queryPackage.PackageId) as ShipmentPackage2;

                            // item cancelado, volta status
                            if (string.IsNullOrEmpty(queryPackage.OuterId) || !resultTracking.ContainsKey(queryPackage.OuterId))
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
                                    Shipment.Comment += $"PROTOCOL: {Package.Protocol} - DELIVERED {Environment.NewLine}";
                                    await TryToSendOrderNotificationsAsync(new OrderNotificationJobArgument[] { new OrderNotificationJobArgument
                                    {
                                        CustomerOrderId = queryPackage.CustomerOrderId,
                                        NotificationTypeName = nameof(OrderDeliveryEmailNotification),
                                        StoreId = activePaymentMethod.StoreId,
                                        CustomerId = queryPackage.CustomerId,
                                    } });
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
                Shipment.Status = K_NewStatus;
                if (!string.IsNullOrEmpty(Package.OuterId))
                {
                    Shipment.Comment += $"{Environment.NewLine}PROTOCOL: {Package.Protocol} - CANCELED {Environment.NewLine}{Environment.NewLine}";
                    Package.TrackingCode = null;
                    Package.OuterId = null;
                }
            }
        }

        public virtual async Task TryToSendOrderNotificationsAsync(OrderNotificationJobArgument[] jobArguments)
        {
            var ordersByIdDict = (await _customerOrderService.GetByIdsAsync(jobArguments.Select(x => x.CustomerOrderId).Distinct().ToArray()))
                                .ToDictionary(x => x.Id)
                                .WithDefaultValue(null);

            foreach (var jobArgument in jobArguments)
            {
                var notification = await _notificationSearchService.GetNotificationAsync(jobArgument.NotificationTypeName, new TenantIdentity(jobArgument.StoreId, nameof(Store)));
                if (notification != null)
                {
                    var order = ordersByIdDict[jobArgument.CustomerOrderId];

                    if (order != null && notification is OrderEmailNotificationBase orderNotification)
                    {
                        var customer = _memberResolver.ResolveMemberByIdAsync(jobArgument.CustomerId).GetAwaiter().GetResult();

                        orderNotification.CustomerOrder = order;
                        orderNotification.Customer = customer;
                        orderNotification.LanguageCode = order.LanguageCode;

                        await SetNotificationParametersAsync(notification, order, customer);
                        await _notificationSender.ScheduleSendNotificationAsync(notification);
                    }
                }
            }
        }

        /// <summary>
        /// Set base notification parameters (sender, recipient, isActive)
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="order"></param>
        protected virtual async Task SetNotificationParametersAsync(Notification notification, CustomerOrder order, Member pCustomer)
        {
            var store = await _storeService.GetByIdAsync(order.StoreId, StoreResponseGroup.StoreInfo.ToString());

            if (notification is EmailNotification emailNotification)
            {
                emailNotification.From = store.EmailWithName;
                emailNotification.To = GetOrderRecipientEmail(order, pCustomer);
            }

            // Allow to filter notification log either by customer order or by subscription
            if (string.IsNullOrEmpty(order.SubscriptionId))
            {
                notification.TenantIdentity = new TenantIdentity(order.Id, nameof(CustomerOrder));
            }
            else
            {
                notification.TenantIdentity = new TenantIdentity(order.SubscriptionId, "Subscription");
            }
        }

        protected virtual string GetOrderRecipientEmail(CustomerOrder order, Member pCustomer)
        {

            var email = GetOrderAddressEmail(order) ?? pCustomer?.Emails?.FirstOrDefault();
            return email;
        }

        protected virtual string GetOrderAddressEmail(CustomerOrder order)
        {
            var email = order.Addresses?.Select(x => x.Email).FirstOrDefault(x => !string.IsNullOrEmpty(x));
            return email;
        }        
    }

    public class OrderNotificationJobArgument
    {
        public string NotificationTypeName { get; set; }
        public string CustomerId { get; set; }
        public string CustomerOrderId { get; set; }
        public string StoreId { get; set; }
    }
}

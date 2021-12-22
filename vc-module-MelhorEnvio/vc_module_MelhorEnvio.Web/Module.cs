using FluentValidation;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using vc_module_MelhorEnvio.Core;
using vc_module_MelhorEnvio.Core.Notifications;
using vc_module_MelhorEnvio.Data.BackgroundJobs;
using vc_module_MelhorEnvio.Data.Handlers;
using vc_module_MelhorEnvio.Data.Model;
using vc_module_MelhorEnvio.Data.Repositories;
using vc_module_MelhorEnvio.Web.Validation;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.NotificationsModule.Core.Services;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.OrdersModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Bus;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Hangfire;
using VirtoCommerce.Platform.Hangfire.Extensions;
using VirtoCommerce.ShippingModule.Core.Services;
using VirtoCommerce.StoreModule.Core.Services;

namespace vc_module_MelhorEnvio.Web
{
    public class Module : IModule
    {
        public ManifestModuleInfo ModuleInfo { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            // initialize DB
            serviceCollection.AddDbContext<ShipmentPackage2DbContext>((provider, options) =>
           {
               var configuration = provider.GetRequiredService<IConfiguration>();
               options.UseSqlServer(configuration.GetConnectionString(ModuleInfo.Id) ?? configuration.GetConnectionString("VirtoCommerce"));
           });

            serviceCollection.AddTransient<IOrderRepository, OrderRepository2>();
            serviceCollection.AddTransient<ShippmendOrderChangedEventHandler>();
            serviceCollection.AddTransient<ShippmendCancelOrderEventHandler>();
            serviceCollection.AddTransient<IValidator<Shipment>, MelhorEnvioValidator>();

            // TODO:
            // serviceCollection.AddTransient<Func<IvcmoduleMelhorEnvioRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<IvcmoduleMelhorEnvioRepository>());
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            // register settings
            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.AllSettings, ModuleInfo.Id);
            settingsRegistrar.RegisterSettingsForType(ModuleConstants.Settings.MelhorEnvio.Settings, nameof(MelhorEnvioMethod));
            settingsRegistrar.RegisterSettingsForType(ModuleConstants.Settings.MelhorEnvio.RestrictSettings, ModuleConstants.objectTypeRestrict);


            var recurringJobManager = appBuilder.ApplicationServices.GetService<IRecurringJobManager>();
            var settingsManager = appBuilder.ApplicationServices.GetRequiredService<ISettingsManager>();
            var memberResolver = appBuilder.ApplicationServices.GetRequiredService<IMemberResolver>();

            recurringJobManager.WatchJobSetting(
                settingsManager,
                new SettingCronJobBuilder()
                    .SetEnablerSetting(ModuleConstants.Settings.MelhorEnvio.EnableSyncJob)
                    .SetCronSetting(ModuleConstants.Settings.MelhorEnvio.CronSyncJob)
                    .ToJob<TrackingJob>(x => x.Process())
                    .Build());

            var notificationRegistrar = appBuilder.ApplicationServices.GetService<INotificationRegistrar>();
            notificationRegistrar.RegisterNotification<OrderDeliveryEmailNotification>();

            // register ShippingMethod
            var shippingMethodsRegistrar = appBuilder.ApplicationServices.GetRequiredService<IShippingMethodsRegistrar>();


            shippingMethodsRegistrar.RegisterShippingMethod(() => new MelhorEnvioMethod(settingsManager, appBuilder.ApplicationServices.GetRequiredService<IStoreService>(), appBuilder.ApplicationServices.GetRequiredService<IFulfillmentCenterService>(), memberResolver));

            var inProcessBus = appBuilder.ApplicationServices.GetService<IHandlerRegistrar>();
            inProcessBus.RegisterHandler<OrderChangedEvent>((message, token) => appBuilder.ApplicationServices.GetService<ShippmendOrderChangedEventHandler>().Handle(message));
            inProcessBus.RegisterHandler<OrderChangedEvent>((message, token) => appBuilder.ApplicationServices.GetService<ShippmendCancelOrderEventHandler>().Handle(message));

            AbstractTypeFactory<ShipmentPackageEntity>.OverrideType<ShipmentPackageEntity, ShipmentPackage2Entity>();
            AbstractTypeFactory<ShipmentPackage>.OverrideType<ShipmentPackage, ShipmentPackage2>();

            // ensure that all pending migrations are applied
            using (var serviceScope = appBuilder.ApplicationServices.CreateScope())
            {
                using (var dbContext = serviceScope.ServiceProvider.GetRequiredService<ShipmentPackage2DbContext>())
                {
                    dbContext.Database.EnsureCreated();
                    dbContext.Database.Migrate();
                }
            }
        }

        public void Uninstall()
        {
            // do nothing in here
        }
    }
}

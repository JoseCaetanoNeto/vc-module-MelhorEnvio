using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using vc_module_MelhorEnvio.Core;
using vc_module_MelhorEnvio.Data.Handlers;
using vc_module_MelhorEnvio.Data.Repositories;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.Platform.Core.Bus;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.ShippingModule.Core.Services;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.Platform.Hangfire;
using VirtoCommerce.Platform.Hangfire.Extensions;
using vc_module_MelhorEnvio.Data.BackgroundJobs;

namespace vc_module_MelhorEnvio.Web
{
    public class Module : IModule
    {
        public ManifestModuleInfo ModuleInfo { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            // initialize DB
            serviceCollection.AddDbContext<vcmoduleMelhorEnvioDbContext>((provider, options) =>
           {
               var configuration = provider.GetRequiredService<IConfiguration>();
               options.UseSqlServer(configuration.GetConnectionString(ModuleInfo.Id) ?? configuration.GetConnectionString("VirtoCommerce"));
           });

            serviceCollection.AddTransient<ShippmendOrderChangedEventHandler>();

            // TODO:
            // serviceCollection.AddTransient<IvcmoduleMelhorEnvioRepository, vcmoduleMelhorEnvioRepository>();
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

            recurringJobManager.WatchJobSetting(
                settingsManager,
                new SettingCronJobBuilder()
                    .SetEnablerSetting(ModuleConstants.Settings.MelhorEnvio.EnableSyncJob)
                    .SetCronSetting(ModuleConstants.Settings.MelhorEnvio.CronSyncJob)
                    .ToJob<TrackingJob>(x => x.Process())
                    .Build());

            // register ShippingMethod
            var shippingMethodsRegistrar = appBuilder.ApplicationServices.GetRequiredService<IShippingMethodsRegistrar>();


            shippingMethodsRegistrar.RegisterShippingMethod(() => new MelhorEnvioMethod(appBuilder.ApplicationServices.GetRequiredService<ISettingsManager>(), appBuilder.ApplicationServices.GetRequiredService<IStoreService>(), appBuilder.ApplicationServices.GetRequiredService<IFulfillmentCenterService>(), appBuilder.ApplicationServices.GetRequiredService<IMemberService>(), appBuilder.ApplicationServices.GetRequiredService<UserManager<ApplicationUser>>()));

            var inProcessBus = appBuilder.ApplicationServices.GetService<IHandlerRegistrar>();
            inProcessBus.RegisterHandler<OrderChangedEvent>((message, token) => appBuilder.ApplicationServices.GetService<ShippmendOrderChangedEventHandler>().Handle(message));


            // ensure that all pending migrations are applied
            using (var serviceScope = appBuilder.ApplicationServices.CreateScope())
            {
                using (var dbContext = serviceScope.ServiceProvider.GetRequiredService<vcmoduleMelhorEnvioDbContext>())
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

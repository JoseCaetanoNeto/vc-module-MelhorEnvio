// Call this to register your module to main application
var moduleName = "vcmoduleMelhorEnvio";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])   
    .run(['platformWebApp.widgetService', 'platformWebApp.toolbarService', 'platformWebApp.authService',
        function (widgetService, toolbarService, authService) {

            var menuItemStore = {
                name: "melhorenviomethod.commands.register",
                icon: 'fa fa-external-link',
                executeMethod: function (blade) {
                    $.getJSON('api/melhorenvio/oauth/authorize/?store=' + blade.storeId, function (url) {
                        window.open(url[0], '_blank');
                    });                    
                },
                canExecuteMethod: function (blade) {
                    return blade.shippingMethod.code == "MelhorEnvioMethod";
                },
                index: 99
            }

            toolbarService.register(menuItemStore, 'virtoCommerce.shippingModule.shippingMethodDetailController');

            //Register dashboard widgets
            //widgetService.registerWidget({
            //    isVisible: function (blade) { return authService.checkPermission('marketing:read'); },
            //    controller: 'virtoCommerce.marketingModule.dashboard.promotionsWidgetController',
            //    template: 'tile-count.html'
            //}, 'mainDashboard');
        }
    ]);

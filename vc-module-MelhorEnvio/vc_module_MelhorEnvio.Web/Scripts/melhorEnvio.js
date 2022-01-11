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

            var menuItemShipmment = {
                name: "melhorenviomethod.commands.traking",
                icon: 'fa fa-external-link',
                executeMethod: function (blade) {

                    for (var i = 0; i < blade.currentEntity.packages.length; i++) {
                        window.open('https://www.melhorrastreio.com.br/rastreio/' + blade.currentEntity.packages[i].trackingCode, '_blank');
                    }

                },
                canExecuteMethod: function (blade) {
                    if (blade.id != "operationDetail" || blade.currentEntity == undefined || blade.currentEntity.operationType != "Shipment") {
                        return false;
                    }
                    if (blade.currentEntity.shippingMethod.code == "MelhorEnvioMethod") {
                        for (var i = 0; i < blade.currentEntity.packages.length; i++) {
                            if (blade.currentEntity.packages[i].trackingCode != undefined) {
                                return true;
                            };
                        }
                    }
                    return false;
                },
                index: 98
            };

            var menuItemOpenCart = {
                name: "melhorenviomethod.commands.open_cart",
                icon: 'fa fa-external-link',
                executeMethod: function (blade) {
                    var sandBox = _.findWhere(blade.currentEntity.shippingMethod.settings, { name: 'vcmoduleMelhorEnvio.sandbox' }).value;
                    window.open(sandBox ? 'https://sandbox.melhorenvio.com.br/carrinho' : 'https://melhorenvio.com.br/carrinho', '_blank');
                },
                canExecuteMethod: function (blade) {
                    if (blade.id != "operationDetail" || blade.currentEntity == undefined || blade.currentEntity.operationType != "Shipment") {
                        return false;
                    }
                    return blade.currentEntity.shippingMethod.code == "MelhorEnvioMethod";
                },
                index: 99
            };

            toolbarService.register(menuItemStore, 'virtoCommerce.shippingModule.shippingMethodDetailController');
            toolbarService.register(menuItemShipmment, 'virtoCommerce.orderModule.operationDetailController');
            toolbarService.register(menuItemOpenCart, 'virtoCommerce.orderModule.operationDetailController');

            //Register dashboard widgets
            //widgetService.registerWidget({
            //    isVisible: function (blade) { return authService.checkPermission('marketing:read'); },
            //    controller: 'virtoCommerce.marketingModule.dashboard.promotionsWidgetController',
            //    template: 'tile-count.html'
            //}, 'mainDashboard');
        }
    ]);

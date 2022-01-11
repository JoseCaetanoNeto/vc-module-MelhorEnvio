// Call this to register your module to main application
var moduleName = "vcmoduleMelhorEnvio";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])   
    .run(['platformWebApp.widgetService', 'platformWebApp.toolbarService', 'platformWebApp.authService', 'platformWebApp.dialogService','platformWebApp.bladeNavigationService',
        function (widgetService, toolbarService, authService, dialogService, bladeNavigationService) {

            var menuItemStore = {
                name: "melhorenvio.commands.register",
                icon: 'fa fa-puzzle-piece',
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

            var menuItemInsertCart = {
                name: "melhorenvio.commands.insert_cart",
                icon: 'fa fa-qrcode',
                executeMethod: function (blade) {
                    var dialog = {
                        id: "confirmDialog",
                        title: "melhorenvio.dialogs.hold-confirmation.title",
                        message:  'melhorenvio.dialogs.hold-confirmation.message',
                        callback: function (ok) {
                            if (ok) {
                                blade.isLoading = true;
                                var sandBox = _.findWhere(blade.currentEntity.shippingMethod.settings, { name: 'vcmoduleMelhorEnvio.sandbox' }).value;
                                $.post('api/melhorenvio/cart', { order_id: blade.customerOrder.id }).then(function () {
                                    blade.isLoading = false;
                                    blade.refresh();
                                    blade.parentBlade.refresh();
                                    bladeNavigationService.closeBlade(blade);
                                    window.open(sandBox ? 'https://sandbox.melhorenvio.com.br/carrinho' : 'https://melhorenvio.com.br/carrinho', '_blank');
                                }).fail(function (response) {
                                    bladeNavigationService.setError('Error ' + response.responseJSON.message, blade);
                                    blade.isLoading = false;
                                });
                            }
                        }
                    };
                    dialogService.showConfirmationDialog(dialog);
                },
                canExecuteMethod: function (blade) {
                    if (blade.id != "operationDetail" || blade.isLoading || blade.isLocked || blade.currentEntity == undefined || blade.currentEntity.operationType != "Shipment") {
                        return false;
                    }
                    if (blade.currentEntity.shippingMethod.code == "MelhorEnvioMethod") {
                        for (var i = 0; i < blade.currentEntity.packages.length; i++) {
                            if (blade.currentEntity.packages[i].outerId == undefined) {
                                return true;
                            };
                        }
                    }
                    return false;
                },
                index: 97
            };

            var menuItemOpenCart = {
                name: "melhorenvio.commands.open_cart",
                icon: 'fa fa-shopping-cart',
                executeMethod: function (blade) {
                    var sandBox = _.findWhere(blade.currentEntity.shippingMethod.settings, { name: 'vcmoduleMelhorEnvio.sandbox' }).value;
                    window.open(sandBox ? 'https://sandbox.melhorenvio.com.br/carrinho' : 'https://melhorenvio.com.br/carrinho', '_blank');
                },
                canExecuteMethod: function (blade) {
                    if (blade.id != "operationDetail" || blade.currentEntity == undefined || blade.currentEntity.operationType != "Shipment") {
                        return false;
                    }
                    if (blade.currentEntity.shippingMethod.code == "MelhorEnvioMethod") {
                        for (var i = 0; i < blade.currentEntity.packages.length; i++) {
                            if (blade.currentEntity.packages[i].trackingCode == undefined && blade.currentEntity.packages[i].outerId != undefined) {
                                return true;
                            };
                        }
                    }
                    return false;
                },
                index: 98
            };           

            var menuItemShipmment = {
                name: "melhorenvio.commands.traking",
                icon: 'fa fa-truck',
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
                index: 99
            };

            toolbarService.register(menuItemStore, 'virtoCommerce.shippingModule.shippingMethodDetailController');
            toolbarService.register(menuItemShipmment, 'virtoCommerce.orderModule.operationDetailController');
            toolbarService.register(menuItemOpenCart, 'virtoCommerce.orderModule.operationDetailController');
            toolbarService.register(menuItemInsertCart, 'virtoCommerce.orderModule.operationDetailController');

            //Register dashboard widgets
            //widgetService.registerWidget({
            //    isVisible: function (blade) { return authService.checkPermission('marketing:read'); },
            //    controller: 'virtoCommerce.marketingModule.dashboard.promotionsWidgetController',
            //    template: 'tile-count.html'
            //}, 'mainDashboard');
        }
    ]);

// Call this to register your module to main application
var moduleName = "vcmoduleMelhorEnvio";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .config(['$stateProvider',
        function ($stateProvider) {
            $stateProvider
                .state('workspace.vcmoduleMelhorEnvioState', {
                    url: '/vcmoduleMelhorEnvio',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: [
                        'platformWebApp.bladeNavigationService', function (bladeNavigationService) {
                            var newBlade = {
                                id: 'blade1',
                                controller: 'vcmoduleMelhorEnvio.helloWorldController',
                                template: 'Modules/$(vcmoduleMelhorEnvio)/Scripts/blades/hello-world.html',
                                isClosingDisabled: true
                            };
                            bladeNavigationService.showBlade(newBlade);
                        }
                    ]
                });
        }
    ])

    .run(['platformWebApp.mainMenuService', 'platformWebApp.widgetService', '$state',
        function (mainMenuService, widgetService, $state) {
            //Register module in main menu
            var menuItem = {
                path: 'browse/vcmoduleMelhorEnvio',
                icon: 'fa fa-cube',
                title: 'vc-module-MelhorEnvio',
                priority: 100,
                action: function () { $state.go('workspace.vcmoduleMelhorEnvioState'); },
                permission: 'vcmoduleMelhorEnvio:access'
            };
            mainMenuService.addMenuItem(menuItem);
        }
    ]);

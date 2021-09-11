angular.module('vcmoduleMelhorEnvio')
    .controller('vcmoduleMelhorEnvio.helloWorldController', ['$scope', 'vcmoduleMelhorEnvio.webApi', function ($scope, api) {
        var blade = $scope.blade;
        blade.title = 'vc-module-MelhorEnvio';

        blade.refresh = function () {
            api.get(function (data) {
                blade.title = 'vcmoduleMelhorEnvio.blades.hello-world.title';
                blade.data = data.result;
                blade.isLoading = false;
            });
        };

        blade.refresh();
    }]);

angular.module('vcmoduleMelhorEnvio')
    .factory('vcmoduleMelhorEnvio.webApi', ['$resource', function ($resource) {
        return $resource('api/vcmoduleMelhorEnvio');
}]);

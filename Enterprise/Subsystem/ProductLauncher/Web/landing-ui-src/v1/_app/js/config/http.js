//  Http Headers Config

(function (angular) {
    "use strict";

    function config($http, rpCookie) {
        var authorization = rpCookie.read('authorization');

        if (authorization !== undefined) {
            authorization = 'Bearer ' + authorization;
            $http.defaults.headers.common.Authorization = authorization;
        }
    }

    angular
        .module("settings")
        .run(['$http', 'rpCookie', config]);
})(angular);


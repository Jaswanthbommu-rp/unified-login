//  Http Headers Config

(function (angular) {
    "use strict";

    function config($http, cookie) {
        var authorization = cookie.read('access_token');

        if (authorization !== undefined) {
            authorization = 'Bearer ' + authorization;
            $http.defaults.headers.common.Authorization = authorization;
        }
    }

    angular
        .module("rpAuthorization")
        .run(['$http', 'rpCookie', config]);
})(angular);


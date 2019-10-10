//  User Custom Fields Service

(function (angular) {
    "use strict";

    function userCustomFields($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/customfields";

        params = {};

        actions = {
            get: {
                method: "GET",
                cancellable: true
            }
        };

        return $resource(url, params, actions);
    }

    angular
        .module("settings")
        .factory("userCustomFieldSvc", ["$resource", "ENV", userCustomFields]);
})(angular);

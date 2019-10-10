//  User Types Service

(function (angular) {
    "use strict";

    function userTypes($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/roleTypes";

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
        .factory("userTypesSvc", ["$resource", "ENV", userTypes]);
})(angular);

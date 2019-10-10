//  User Type Options Service

(function (angular) {
    "use strict";

    function userTypeOptions($resource, ENV) {
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
        .factory("userTypeOptionsSvc", ["$resource", "ENV", userTypeOptions]);
})(angular);

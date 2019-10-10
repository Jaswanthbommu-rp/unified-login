//  Dashboard Service

(function (angular) {
    "use strict";

    function dashboardSvc($resource, ENV) {
        var url, defaults, actions;

        actions = {
            get: {
                method: "GET",
                cancellable: true
            }
        };

        defaults = {};

        url = ENV.landingAPI + "api/dashboard";

        return $resource(url, defaults, actions);
    }

    angular
        .module("settings")
        .factory("dashboardSvc", ["$resource", "ENV", dashboardSvc]);
})(angular);

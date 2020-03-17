//   Aside Service

(function(angular) {
    "use strict";

    function asideSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "/api/products/unifiedlogin/role/rights";
      

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
        .factory("asideSvc", ["$resource", "ENV", asideSvc]);
})(angular);

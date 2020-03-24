//  Controls Service

(function(angular) {
    "use strict";

    function ProductControlsSvc($resource, ENV) {
        var url, params,actions;
        url = ENV.controlsAPI + "v2/UserMgmt/ProductAccess";

        params = {
            productId: "@productId"
        };

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
        .factory("productControlsSvc", [
            "$resource",
            "ENV",
            ProductControlsSvc
        ]);
})(angular);

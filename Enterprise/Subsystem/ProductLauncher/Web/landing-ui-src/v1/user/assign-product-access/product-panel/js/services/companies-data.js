//  Propeties Service

(function(angular) {
    "use strict";

    function ProductCompaniesSvc($resource, ENV) {
        var url, params,actions;
        url = ENV.landingAPI + "api/product/companies";

        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
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
        .factory("ProductCompaniesSvc", [
            "$resource",
            "ENV",
            ProductCompaniesSvc
        ]);
})(angular);

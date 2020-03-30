//  Propeties Service

(function(angular) {
    "use strict";

    function ProductPropertiesSvc($resource, ENV) {
        var url, params,actions;
        url = ENV.landingAPI + "api/product/properties";

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
        .factory("productPropertiesSvc", [
            "$resource",
            "ENV",
            ProductPropertiesSvc
        ]);
})(angular);

(function(angular) {
    "use strict";

    function ProductGroupPropertiesSvc($resource, ENV) {
        var url, params,actions;
        url = ENV.landingAPI + "api/product/groupproperties";

        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
            productId: "@productId",
            propertyGroupId: "@propertyGroupId"
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
        .factory("productGroupPropertiesSvc", [
            "$resource",
            "ENV",
            ProductGroupPropertiesSvc
        ]);
})(angular);

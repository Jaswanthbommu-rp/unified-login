//  Asset Optimization Propeties Service

(function(angular) {
    "use strict";

    function AssetOptimizationPropertiesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/ao/companyproperties";

        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
            productName: "@productDivisionName"
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
        .factory("AssetOptimizationPropertiesSvc", [
            "$resource",
            "ENV",
            AssetOptimizationPropertiesSvc
        ]);
})(angular);

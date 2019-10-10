//  Asset Optimization Company Service

(function(angular) {
    "use strict";

    function AssetOptimizationCompanySvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/ao/companies";

        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
            productName: "@productName"
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
        .factory("AssetOptimizationCompanySvc", [
            "$resource",
            "ENV",
            AssetOptimizationCompanySvc
        ]);
})(angular);

//  Asset Optimization Roles Service

(function (angular) {
    "use strict";

    function AssetOptimizationRolesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/ao/companyroles";

        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
            productName: "@productDivisionName",
            userLoginName: "@userLoginName"
        };
        logc(params);
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
        .factory("AssetOptimizationRolesSvc", [
            "$resource",
            "ENV",
            AssetOptimizationRolesSvc
        ]);
})(angular);

//  Asset Optimization Property Group Service

(function(angular) {
    "use strict";

    function AssetOptimizationPropertyGroupSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/ao/propertygroups";

        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
            productName: "@productName",
            selectedCompanies: "@selectedCompanies",
            userLoginName: "@userLoginName"
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
        .factory("AssetOptimizationPropertyGroupSvc", [
            "$resource",
            "ENV",
            AssetOptimizationPropertyGroupSvc
        ]);
})(angular);

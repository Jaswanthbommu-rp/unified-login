//  Asset Optimization Property Group Service

(function(angular) {
    "use strict";

    function AssetOptimizationGroupPropertyListSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/ao/groupproperties";

        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId:   "@userPersonalID",
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
        .factory("AssetOptimizationGroupPropertyListSvc", [
            "$resource",
            "ENV",
            AssetOptimizationGroupPropertyListSvc
        ]);
})(angular);

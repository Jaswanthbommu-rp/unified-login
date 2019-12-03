//  Asset Optimization Property Group Service

(function(angular) {
    "use strict";

    function AssetOptimizationCompanyPropertyListSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/ao/companyproperties";

        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId:   "@userPersonalID",
            productName:     "@productName",
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
        .factory("AssetOptimizationCompanyPropertyListSvc", [
            "$resource",
            "ENV",
            AssetOptimizationCompanyPropertyListSvc
        ]);
})(angular);

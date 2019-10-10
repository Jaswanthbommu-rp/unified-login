//  Utility Management Regions Service

(function(angular) {
    "use strict";

    function UtilityManagementRegionsSvc($resource, ENV) {
        var url, params, actions;


        url = ENV.landingAPI + "api/products/rum/regions";

        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID"
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
        .factory("UtilityManagementRegionsSvc", [
            "$resource",
            "ENV",
            UtilityManagementRegionsSvc
        ]);
})(angular);

//  Utility Management Propeties Service

(function(angular) {
    "use strict";

    function UtilityManagementPropertiesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/rum/properties";

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
        .factory("UtilityManagementPropertiesSvc", [
            "$resource",
            "ENV",
            UtilityManagementPropertiesSvc
        ]);
})(angular);

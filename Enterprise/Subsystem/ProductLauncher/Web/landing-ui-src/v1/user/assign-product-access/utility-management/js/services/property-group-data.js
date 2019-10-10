//  Utility Management Propety Group Service

(function(angular) {
    "use strict";

    function UtilityManagementPropertyGroupSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/rum/propertygroups";

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
        .factory("UtilityManagementPropertyGroupSvc", [
            "$resource",
            "ENV",
            UtilityManagementPropertyGroupSvc
        ]);
})(angular);

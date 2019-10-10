//  Utility Management Roles Service

(function(angular) {
    "use strict";

    function UtilityManagementRolesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/rum/roles";

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
        .factory("UtilityManagementRolesSvc", [
            "$resource",
            "ENV",
            UtilityManagementRolesSvc
        ]);
})(angular);

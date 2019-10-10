//  Roles Service

(function(angular) {
    "use strict";

    function ILMLeadManagementRolesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/ilm/roles";

        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
            assignedOnly: "false",
            accessType: "Property"
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
        .factory("ILMLeadManagementRolesSvc", [
            "$resource",
            "ENV",
            ILMLeadManagementRolesSvc
        ]);
})(angular);

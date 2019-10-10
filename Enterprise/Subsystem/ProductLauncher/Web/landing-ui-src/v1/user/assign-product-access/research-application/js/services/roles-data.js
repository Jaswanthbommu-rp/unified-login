//  res-app Roles Service

(function(angular) {
    "use strict";

    function ResAppUserRolesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/researchapplication/roles";

        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
            partyId: "@partyID",
            assignedOnly: "false"
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
        .factory("resAppUserRolesSvc", [
        	"$resource",
            "ENV",
        	ResAppUserRolesSvc
        ]);
})(angular);

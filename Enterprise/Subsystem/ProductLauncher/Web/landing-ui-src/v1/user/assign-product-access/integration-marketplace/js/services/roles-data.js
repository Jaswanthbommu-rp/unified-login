//  IntegMkt Roles Service

(function(angular) {
    "use strict";

    function IntegMktUserRolesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/integrationmarketplace/roles";

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
        .factory("integMktUserRolesSvc", [
        	"$resource",
            "ENV",
        	IntegMktUserRolesSvc
        ]);
})(angular);

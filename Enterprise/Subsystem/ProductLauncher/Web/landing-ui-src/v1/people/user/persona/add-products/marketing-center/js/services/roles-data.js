//  Marketing Center Roles Service

(function(angular) {
    "use strict";

    function MCRolesSvc($resource, ENV) {
        var url, params, actions;
        url = ENV.landingAPI + "/api/products/marketingcenter/roles";
        
        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
            assignedOnly: "false",
        };

        actions = {
            get: {
                method: "GET",
                cancellable : true
            }
        };

        return $resource(url, params, actions);
    }

    angular
        .module("settings")
        .factory("MCRolesSvc", [
        	"$resource",
            "ENV", 
        	MCRolesSvc
        ]);
})(angular);

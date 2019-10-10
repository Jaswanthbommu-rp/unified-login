(function(angular) {
    "use strict";

    function ClientPortalRolesSvc($resource,ENV) {
        var url, params,actions;
       url = ENV.landingAPI + "/api/products/clientportal/roles";
        
        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
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
        .factory("clientPortalRolesSvc", [
        	"$resource", 
            "ENV",
             ClientPortalRolesSvc
        ]);
})(angular);

//  Spend Management Roles Service

(function(angular) {
    "use strict";

    function SMRolesSvc($resource, ENV) {
        var url, params;
        url = ENV.landingAPI + "/api/products/ops/roles";
        
        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
            assignedOnly: "false"
        };

        return $resource(url, params);
    }

    angular
        .module("settings")
        .factory("SMRolesSvc", [
        	"$resource", 
            "ENV",
        	SMRolesSvc
        ]);
})(angular);

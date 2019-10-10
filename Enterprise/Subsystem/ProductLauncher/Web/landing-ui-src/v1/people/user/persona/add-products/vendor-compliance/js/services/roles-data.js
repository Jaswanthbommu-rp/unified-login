//  Roles Service

(function(angular) {
    "use strict";

    function VendCompRolesSvc($resource, ENV) { 
        var url, params;
        url = ENV.landingAPI + "/api/products/vendorcompliance/roles";
        
        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
            assignedOnly: "false"
        };

        return $resource(url, params);
    }

    angular
        .module("settings")
        .factory("VendCompRolesSvc", [
        	"$resource", 
            "ENV",
        	VendCompRolesSvc
        ]);
})(angular);

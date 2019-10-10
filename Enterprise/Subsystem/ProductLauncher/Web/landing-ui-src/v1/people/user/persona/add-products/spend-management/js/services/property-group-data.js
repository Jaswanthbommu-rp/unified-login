//  Contact Center Property Group Service

(function(angular) {
    "use strict";

    function SMPropertyGroupSvc($resource, ENV) {
        var url, params;
        url = ENV.landingAPI + "/api/products/ops/assets";
        
        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
            assignedOnly: "false"
        };

        return $resource(url, params);
    }

    angular
        .module("settings")
        .factory("SMPropertyGroupSvc", [
        	"$resource", 
            "ENV",
        	SMPropertyGroupSvc
        ]);
})(angular);

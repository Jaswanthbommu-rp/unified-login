//  Property Group Service

(function(angular) {
    "use strict";

    function VendCompPropertyGroupSvc($resource, ENV) {
        var url, params;
        url = ENV.landingAPI + "/api/products/vendorcompliance/propertygroups";
        
        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID"
        };

        return $resource(url, params);
    }

    angular
        .module("settings")
        .factory("VendCompPropertyGroupSvc", [
        	"$resource", 
            "ENV",
        	VendCompPropertyGroupSvc
        ]);
})(angular);

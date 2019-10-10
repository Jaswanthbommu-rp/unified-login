//  Propeties Service

(function(angular) {
    "use strict";

    function VendCompPropertiesSvc($resource, ENV) {
        var url, params;
        url = ENV.landingAPI + "/api/products/vendorcompliance/properties";
        
        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID"
        };

        return $resource(url, params);
    }

    angular
        .module("settings")
        .factory("VendCompPropertiesSvc", [
        	"$resource", 
            "ENV",
        	VendCompPropertiesSvc
        ]);
})(angular);

//  Propeties Service

(function(angular) {
    "use strict";

    function ClientPortalPropertiesSvc($resource, ENV) {
        var url, params,actions;
        url = ENV.landingAPI + "/api/products/clientportal/properties";
        
        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID"
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
        .factory("clientPortalPropertiesSvc", [
        	"$resource", 
            "ENV",
        	ClientPortalPropertiesSvc
        ]);
})(angular);

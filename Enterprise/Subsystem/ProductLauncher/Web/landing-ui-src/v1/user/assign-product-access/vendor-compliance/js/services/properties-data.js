//  Propeties Service

(function(angular) {
    "use strict";

    function VendCompPropertiesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/vendorcompliance/properties";

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
        .factory("VendCompPropertiesSvc", [
        	"$resource",
            "ENV",
        	VendCompPropertiesSvc
        ]);
})(angular);

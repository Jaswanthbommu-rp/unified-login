//  Marketing Center Propeties Service

(function(angular) {
    "use strict";

    function MCPropertiesSvc($resource, ENV) {
        var url, params, actions;
        url = ENV.landingAPI + "api/products/marketingcenter/properties";

        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
            assignedOnly: "false"
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
        .factory("MCPropertiesSvc", [
        	"$resource",
            "ENV",
        	MCPropertiesSvc
        ]);
})(angular);

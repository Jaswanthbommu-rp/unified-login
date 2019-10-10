//  L2L Properties Service

(function(angular) {
    "use strict";

    function lead2LeasePropertiesSvc($resource, ENV) {
        var url, params, actions;
        url = ENV.landingAPI + "api/products/lead2lease/properties";

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
        .factory("lead2LeasePropertiesSvc", ["$resource", "ENV", lead2LeasePropertiesSvc]);
})(angular);

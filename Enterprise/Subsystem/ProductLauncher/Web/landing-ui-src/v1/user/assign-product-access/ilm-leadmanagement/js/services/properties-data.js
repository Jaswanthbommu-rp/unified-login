//  Propeties Service

(function(angular) {
    "use strict";

    function ILMLeadManagementPropertiesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/ilm/properties";

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
        .factory("ILMLeadManagementPropertiesSvc", [
            "$resource",
            "ENV",
            ILMLeadManagementPropertiesSvc
        ]);
})(angular);

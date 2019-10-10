//  Onesite Properties Service

(function (angular) {
    "use strict";

    function DAPropertiesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/properties";

        params = {
            productType: "DepositAlternative",
            editorPersonaId: "@editorPersonaId",
            subjectPersonaId: "@userPersonaId",            
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
        .factory("DAPropertiesSvc", [
            "$resource",
            "ENV",
            DAPropertiesSvc
        ]);
})(angular);

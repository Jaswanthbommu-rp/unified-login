//  Properties Service

(function (angular) {
    "use strict";

    function RentersInsurancePropertiesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/rentersinsurance/properties";

        params = {
            editorPersonaId: "@editorPersonaId",
            userPersonaId: "@userPersonaId"
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
        .factory("RentersInsurancePropertiesSvc", [
            "$resource",
            "ENV",
            RentersInsurancePropertiesSvc
        ]);
})(angular);

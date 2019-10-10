//  Renters Insurnace Roles Service

(function (angular) {
    "use strict";

    function RentersInsuranceRolesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/rentersinsurance/roles";

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
        .factory("RentersInsuranceRolesSvc", [
            "$resource",
            "ENV",
            RentersInsuranceRolesSvc
        ]);
})(angular);

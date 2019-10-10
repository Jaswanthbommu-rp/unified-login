//  Unified Amenities Roles Service

(function (angular) {
    "use strict";

    function UARolesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/unifiedamenities/roles";

        params = {
            editorPersonaId: "@editorPersonaId",
            userPersonaId: "@userPersonaId",
            partyId: "@partyId"
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
        .factory("UARolesSvc", [
            "$resource",
            "ENV",
            UARolesSvc
        ]);
})(angular);

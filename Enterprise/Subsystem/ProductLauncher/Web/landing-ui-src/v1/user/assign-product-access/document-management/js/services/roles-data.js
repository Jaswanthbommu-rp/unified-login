//  Onesite Roles Service

(function (angular) {
    "use strict";

    function DMRolesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/rpdm/roles";

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
        .factory("DMRolesSvc", [
            "$resource",
            "ENV",
            DMRolesSvc
        ]);
})(angular);

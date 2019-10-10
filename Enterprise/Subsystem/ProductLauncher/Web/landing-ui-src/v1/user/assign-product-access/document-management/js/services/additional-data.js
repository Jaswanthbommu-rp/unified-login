//  Onesite Roles Service

(function (angular) {
    "use strict";

    function DMAdditionalDataSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/rpdm/role/classifier";

        params = {
            editorPersonaId: "@editorPersonaId",
            userPersonaId: "@userPersonaId",
            roleId: "@roleId"
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
        .factory("DMAdditionalDataSvc", [
            "$resource",
            "ENV",
            DMAdditionalDataSvc
        ]);
})(angular);

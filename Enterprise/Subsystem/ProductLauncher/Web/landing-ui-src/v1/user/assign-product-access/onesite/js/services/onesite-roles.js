//  Onesite Roles Service

(function (angular) {
    "use strict";

    function OSRolesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/onesite/user/roles";

        params = {
            editorPersonaId: "@editorPersonaId",
            userPersonaId: "@userPersonaId",
            assignedOnly: "false"
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
        .factory("OSRolesSvc", [
			"$resource",
			"ENV",
			OSRolesSvc
        ]);
})(angular);

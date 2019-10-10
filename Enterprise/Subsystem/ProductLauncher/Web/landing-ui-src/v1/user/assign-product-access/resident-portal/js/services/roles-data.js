//  Onesite Roles Service

(function (angular) {
    "use strict";

    function RPRolesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/residentportal/levels";

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
        .factory("RPRolesSvc", [
			"$resource",
			"ENV",
			RPRolesSvc
        ]);
})(angular);

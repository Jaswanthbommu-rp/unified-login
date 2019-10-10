//  Onesite Roles Service

(function (angular) {
    "use strict";

    function OSRolesSvc($resource, ENV) {
        var url, params;
        url = ENV.landingAPI + "/api/products/onesite/user/roles";

        params = {
            editorPersonaId: "@editorPersonaId",
            userPersonaId: "@userPersonaId",
            assignedOnly: "false"
        };

        return $resource(url, params);
    }

    angular
        .module("settings")
        .factory("OSRolesSvc", [
			"$resource",
			"ENV",
			OSRolesSvc
        ]);
})(angular);

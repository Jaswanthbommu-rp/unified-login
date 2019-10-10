//  Onesite Properties Service

(function (angular) {
    "use strict";

    function OSPropertiesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/onesite/user/properties";

        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
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
        .factory("OSPropertiesSvc", [
			"$resource",
			"ENV",
			OSPropertiesSvc
        ]);
})(angular);

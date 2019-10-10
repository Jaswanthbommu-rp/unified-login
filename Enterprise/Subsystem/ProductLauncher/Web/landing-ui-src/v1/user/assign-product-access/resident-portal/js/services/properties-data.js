//  Resident Portals Properties Service

(function (angular) {
    "use strict";

    function RPPropertiesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/residentportal/properties";

        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
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
        .factory("RPPropertiesSvc", [
			"$resource",
			"ENV",
			RPPropertiesSvc
        ]);
})(angular);

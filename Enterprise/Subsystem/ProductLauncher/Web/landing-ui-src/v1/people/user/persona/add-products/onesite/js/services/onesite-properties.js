//  Onesite Properties Service

(function (angular) {
    "use strict";

    function OSPropertiesSvc($resource, ENV) {
        var url, params;
        url = ENV.landingAPI + "/api/products/onesite/user/properties";
        
        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
            assignedOnly: "false"
        };

        return $resource(url, params);
    }

    angular
        .module("settings")
        .factory("OSPropertiesSvc", [
			"$resource",
			"ENV",
			OSPropertiesSvc
        ]);
})(angular);

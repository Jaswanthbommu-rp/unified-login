// Generic Product Properties Groups Service

(function(angular) {
    "use strict";

    function ProductGroupPropertiesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/properties?groupId";

        params = {
            productType: "@productType",
            editorPersonaId: "@editorPersonalID",
            subjectPersonaId: "@subjectPersonaId",
            groupId: "@groupId"
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
        .factory("ProductGroupPropertiesSvc", [
            "$resource",
            "ENV",
            ProductGroupPropertiesSvc
        ]);
})(angular);

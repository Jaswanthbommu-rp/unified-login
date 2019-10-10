// Generic Product Properties Service

(function(angular) {
    "use strict";

    function ProductPropertiesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/properties";

        params = {
            productType: "@productType",
            editorPersonaId: "@editorPersonalID",
            subjectPersonaId: "@subjectPersonaId"
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
        .factory("ProductPropertiesSvc", [
            "$resource",
            "ENV",
            ProductPropertiesSvc
        ]);
})(angular);

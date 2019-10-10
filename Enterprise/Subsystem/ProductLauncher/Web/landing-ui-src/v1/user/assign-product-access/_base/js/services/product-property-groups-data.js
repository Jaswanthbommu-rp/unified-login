// Generic Product Properties Groups Service

(function(angular) {
    "use strict";

    function ProductPropertyGroupsSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/propertygroups";

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
        .factory("ProductPropertyGroupsSvc", [
            "$resource",
            "ENV",
            ProductPropertyGroupsSvc
        ]);
})(angular);

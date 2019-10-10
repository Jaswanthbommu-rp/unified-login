// Generic Product Roles Service

(function(angular) {
    "use strict";

    function ProductRolesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/roles";

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
        .factory("ProductRolesSvc", [
            "$resource",
            "ENV",
            ProductRolesSvc
        ]);
})(angular);

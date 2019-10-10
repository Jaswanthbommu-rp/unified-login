//  Onesite Properties Service

(function (angular) {
    "use strict";

    function DAAreasSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/propertygroups";

        params = {
            productType: "DepositAlternative",
            editorPersonaId: "@editorPersonaId",
            subjectPersonaId: "@userPersonaId",            
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
        .factory("DAAreasSvc", [
            "$resource",
            "ENV",
            DAAreasSvc
        ]);
})(angular);

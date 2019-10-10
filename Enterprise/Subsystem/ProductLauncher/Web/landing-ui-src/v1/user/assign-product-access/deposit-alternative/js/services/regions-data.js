//  Onesite Properties Service

(function (angular) {
    "use strict";

    function DARegionsSvc($resource, ENV) {
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
        .factory("DARegionsSvc", [
            "$resource",
            "ENV",
            DARegionsSvc
        ]);
})(angular);

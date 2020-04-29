(function(angular) {
    "use strict";

    function ProductPropertyGroupSvc($resource, ENV) {
        var url, params,actions;
        url = ENV.landingAPI + "api/product/propertygroups";

        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
            productId: "@productId"
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
        .factory("productPropertyGroupSvc", [
            "$resource",
            "ENV",
            ProductPropertyGroupSvc
        ]);
})(angular);

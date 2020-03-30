(function(angular) {
    "use strict";

    function ProductRolesSvc($resource,ENV) {
        var url, params,actions;
       url = ENV.landingAPI + "api/product/roles";
        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
            partyId: "@partyID",
            productId: "@productId",
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
        .factory("productRolesSvc", [
            "$resource",
            "ENV",
             ProductRolesSvc
        ]);
})(angular);

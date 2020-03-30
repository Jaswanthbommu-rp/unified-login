(function(angular) {
    "use strict";

    function ProductRightsSvc($resource,ENV) {
        var url, params,actions;
       url = ENV.landingAPI + "api/product/rightsforrole";
        params = {
            editorPersonaId: "@editorPersonalID",
            productId: "@productId",
            roleId: "@roleID",
            partyId: "@partyID",
            assignedToRoleOnly: "@assignedToRoleOnly"
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
        .factory("productRightsSvc", [
            "$resource",
            "ENV",
             ProductRightsSvc
        ]);
})(angular);

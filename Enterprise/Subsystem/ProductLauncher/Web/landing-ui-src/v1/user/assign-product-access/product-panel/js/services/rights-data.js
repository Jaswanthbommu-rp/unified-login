(function(angular) {
    "use strict";

    function ProductRightsSvc($resource,ENV) {
        var url, params,actions;
       url = ENV.landingAPI + "api/product/productrights";
        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
            partyId: "@partyID",
            productId: "@productId",
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

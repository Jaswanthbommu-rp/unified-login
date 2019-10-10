//  Deposit Alt Roles Service

(function (angular) {
    "use strict";

    function DARolesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/roles";

         params = {
            productType: "ClickPay",
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
        .factory("DARolesSvc", [
            "$resource",
            "ENV",
            DARolesSvc
        ]);
})(angular);

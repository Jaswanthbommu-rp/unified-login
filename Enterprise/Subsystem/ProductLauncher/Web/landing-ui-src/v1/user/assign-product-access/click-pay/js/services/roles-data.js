//  Click Pay Roles Service

(function (angular) {
    "use strict";

    function CPCompRolesSvc($resource, ENV) {
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
        .factory("CPCompRolesSvc", [
            "$resource",
            "ENV",
            CPCompRolesSvc
        ]);
})(angular);

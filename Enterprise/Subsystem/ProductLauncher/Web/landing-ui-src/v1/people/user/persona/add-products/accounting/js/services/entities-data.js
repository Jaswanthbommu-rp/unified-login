//  Accounting Entities Service

(function(angular) {
    "use strict";

    function AEntitiesSvc($resource, ENV) {
        var url, params, actions;
        url = ENV.landingAPI + "/api/products/onesiteaccounting/user/properties";
        
        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
            assignedOnly: "false"
        };

        actions = {
            get: {
                method: "GET",
                cancellable : true
            }
        };

        return $resource(url, params, actions);
    }

    angular
        .module("settings")
        .factory("AEntitiesSvc", ["$resource", "ENV", AEntitiesSvc]);
})(angular);

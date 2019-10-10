//  Accounting Roles Service

(function(angular) {
    "use strict";

    function ARolesSvc($resource, ENV) {
        var url, params, actions;
        url = ENV.landingAPI + "/api/products/onesiteaccounting/user/roles";
        
        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID"
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
        .factory("ARolesSvc", ["$resource", "ENV", ARolesSvc]);
})(angular);

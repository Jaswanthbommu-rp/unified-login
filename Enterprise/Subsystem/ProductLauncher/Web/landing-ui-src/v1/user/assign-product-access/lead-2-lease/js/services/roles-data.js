//  L2L Roles Service

(function(angular) {
    "use strict";

    function lead2LeaseRolesSvc($resource, ENV) {
        var url, params, actions;
        url = ENV.landingAPI + "api/products/lead2lease/roles";

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
        .factory("lead2LeaseRolesSvc", ["$resource", "ENV", lead2LeaseRolesSvc]);
})(angular);

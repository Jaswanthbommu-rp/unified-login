//  OnSite Roles Service

(function(angular) {
    "use strict";

    function onSiteRolesSvc($resource, ENV) {
        var url, params, actions;
        url = ENV.landingAPI + "api/products/onsite/roles";

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
        .factory("onSiteRolesSvc", ["$resource", "ENV", onSiteRolesSvc]);
})(angular);

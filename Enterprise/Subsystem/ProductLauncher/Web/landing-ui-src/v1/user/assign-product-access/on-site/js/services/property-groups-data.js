//  OnSite PropertyGroups Service

(function(angular) {
    "use strict";

    function onSitePropertyGroupsSvc($resource, ENV) {
        var url, params, actions;
        url = ENV.landingAPI + "api/products/onsite/regions";

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
        .factory("onSitePropertyGroupsSvc", ["$resource", "ENV", onSitePropertyGroupsSvc]);
})(angular);

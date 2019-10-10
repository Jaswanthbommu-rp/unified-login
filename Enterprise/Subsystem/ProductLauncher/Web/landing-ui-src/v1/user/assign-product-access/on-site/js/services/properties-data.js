//  OnSite Properties Service

(function(angular) {
    "use strict";

    function onSitePropertiesSvc($resource, ENV) {
        var url, params, actions;
        url = ENV.landingAPI + "api/products/onsite/properties";

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
        .factory("onSitePropertiesSvc", ["$resource", "ENV", onSitePropertiesSvc]);
})(angular);

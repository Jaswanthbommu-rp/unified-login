//  Resident Portals Notifications Service

(function(angular) {
    "use strict";

    function resPortNotificationsSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/residentportal/notifications";

        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID"
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
        .factory("resPortNotificationsSvc", [
            "$resource",
            "ENV",
            resPortNotificationsSvc
        ]);
})(angular);

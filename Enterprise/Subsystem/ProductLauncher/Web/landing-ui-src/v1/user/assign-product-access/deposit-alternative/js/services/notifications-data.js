//  Notifications Service

(function(angular) {
    "use strict";

    function daNotificationsSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/vendorcompliance/notifications";

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
        .factory("daNotificationsSvc", [
            "$resource",
            "ENV",
            daNotificationsSvc
        ]);
})(angular);

//  Resident Portals MessagingGroups Service

(function(angular) {
    "use strict";

    function RPMessagingGroupsSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/residentportal/messagegroups";

        params = {
            editorPersonaId: "@editorPersonaId",
            userPersonaId: "@userPersonaId",
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
        .factory("RPMessagingGroupsSvc", [
            "$resource",
            "ENV",
            RPMessagingGroupsSvc
        ]);
})(angular);

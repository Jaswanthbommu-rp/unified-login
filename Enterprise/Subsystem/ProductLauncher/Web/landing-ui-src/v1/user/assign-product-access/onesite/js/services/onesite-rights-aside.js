//  OneSite Rights List Aside Service

(function(angular) {
    "use strict";

    function OSRightsListAsideSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/onesite/rights";

        params = {
            roleId: "@roleId",
            assignedToRoleOnly: "true",
            editorPersonaId: "@editorPersonaId"
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
        .factory("OSRightsListAsideSvc", ["$resource", "ENV", OSRightsListAsideSvc]);
})(angular);

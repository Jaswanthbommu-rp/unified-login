//  OneSite Rights List Aside Service

(function(angular) {
    "use strict";

    function OSRightsListAsideSvc($resource, ENV) {
        var url, params;
        url = ENV.landingAPI + "api/products/onesite/rights";

        params = {
            editorPersonaId: "@editorPersonaId",
            roleId: "@roleId",
            assignedToRoleOnly: "true"
        };


        return $resource(url, params);
    }

    angular
        .module("settings")
        .factory("OSRightsListAsideSvc", ["$resource", "ENV", OSRightsListAsideSvc]);
})(angular);

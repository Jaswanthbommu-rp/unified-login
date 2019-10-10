//  Unified Amenitites Rights List Aside Service

(function (angular) {
    "use strict";

    function UARightsListAsideSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/unifiedamenities/role/rights";

        params = {
            roleId: "@roleId",
            partyId: "@partyId",
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
        .factory("UARightsListAsideSvc", ["$resource", "ENV", UARightsListAsideSvc]);
})(angular);

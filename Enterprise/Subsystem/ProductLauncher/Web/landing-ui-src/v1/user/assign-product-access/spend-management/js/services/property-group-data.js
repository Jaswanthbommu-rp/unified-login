//  Contact Center Property Group Service

(function(angular) {
    "use strict";

    function SMPropertyGroupSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/ops/assets";

        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
            assignedOnly: "false"
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
        .factory("SMPropertyGroupSvc", [
        	"$resource",
            "ENV",
        	SMPropertyGroupSvc
        ]);
})(angular);

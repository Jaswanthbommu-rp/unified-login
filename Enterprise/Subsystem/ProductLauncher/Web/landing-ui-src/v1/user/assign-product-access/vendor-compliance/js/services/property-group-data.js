//  Property Group Service

(function(angular) {
    "use strict";

    function VendCompPropertyGroupSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/vendorcompliance/propertygroups";

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
        .factory("VendCompPropertyGroupSvc", [
        	"$resource",
            "ENV",
        	VendCompPropertyGroupSvc
        ]);
})(angular);

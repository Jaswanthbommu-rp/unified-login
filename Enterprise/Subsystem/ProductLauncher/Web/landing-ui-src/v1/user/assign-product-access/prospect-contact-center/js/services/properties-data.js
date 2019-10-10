//  PCC Properties Service

(function(angular) {
    "use strict";

    function prospectContactCenterPropertiesSvc($resource, ENV) {
        var url, params, actions;
        url = ENV.landingAPI + "api/products/prospectcontactcenter/properties";

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
        .factory("prospectContactCenterPropertiesSvc", ["$resource", "ENV", prospectContactCenterPropertiesSvc]);
})(angular);

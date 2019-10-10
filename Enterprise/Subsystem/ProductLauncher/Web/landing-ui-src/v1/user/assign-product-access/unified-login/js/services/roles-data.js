//  UserMgmt Roles Service

(function(angular) {
    "use strict";

    function UserMgmtUserRolesSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/products/unifiedlogin/user/roles";

        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
            partyId: "@partyID",
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
        .factory("userMgmtUserRolesSvc", [
        	"$resource",
            "ENV",
        	UserMgmtUserRolesSvc
        ]);
})(angular);

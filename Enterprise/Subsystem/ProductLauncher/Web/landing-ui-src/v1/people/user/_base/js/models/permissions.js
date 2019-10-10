//  Manage User Permissions Model

(function (angular, undefined) {
    "use strict";

    function factory(userTypes) {
        var model = {},
            permissions = {},
            defaultUserPermissions = {
                //display permissions
                hasAddPersonaBtn: true,
                hasEnterpriseRole: true,
                hasNotificationEmail: false,
                hasUsernameOnly: false
            };

        model.init = function() {
            permissions.defaultUser = angular.copy(defaultUserPermissions);

            //Regular User with No Email
            var regularUserNoEmailID = "_" + userTypes.REGULAR_NO_EMAIL.id;
            permissions[regularUserNoEmailID] = angular.copy(defaultUserPermissions);
            permissions[regularUserNoEmailID].hasNotificationEmail = true;
            permissions[regularUserNoEmailID].hasUsernameOnly = true;

            //SDE User
            var sdeID = "_" + userTypes.SDE.id;
            permissions[sdeID] = angular.copy(defaultUserPermissions);
            permissions[sdeID].hasAddPersonaBtn = false;
            permissions[sdeID].hasEnterpriseRole = false;
            permissions[sdeID].hasUsernameOnly = true;

            model.permissions = permissions;

            return model;
        };

        model.get = function (key) {
            var userTypeKey = "_" + key; //prepend to avoid being treated as an array index
            if (!permissions[userTypeKey]) {
                userTypeKey = "defaultUser";
            }

            return permissions[userTypeKey];
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("manageUserPermissionsModel", [
            "userTypes",
            factory
        ]);
})(angular);

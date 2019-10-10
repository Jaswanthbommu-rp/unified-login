//  Roles And Rights Tabs Data Service

(function(angular, undefined) {
    "use strict";

    function RolesRightsTabsData() {
        var svc = this;

        svc.data = {
            rights: {
                id: "rights",
                text: "Rights",
                isActive: false,
                sref: "roles-and-rights.rights"
            },

            roles: {
                id: "roles",
                text: "Roles",
                isActive: true,
                sref: "roles-and-rights.roles"
            }

            // entRoles: {
            //     id: "entRoles",
            //     isActive: false,
            //     text: "Enterprise Roles",
            //     sref: "roles-and-rights.enterprise-roles"
            // }
        };

        svc.getTab = function(tabId) {
            return svc.data[tabId];
        };

        svc.getList = function() {
            return [
                svc.data.rights,
                svc.data.roles
                // svc.data.entRoles
            ];
        };
    }

    angular
        .module("settings")
        .service("rolesRightsTabsData", [
            RolesRightsTabsData
        ]);
})(angular);
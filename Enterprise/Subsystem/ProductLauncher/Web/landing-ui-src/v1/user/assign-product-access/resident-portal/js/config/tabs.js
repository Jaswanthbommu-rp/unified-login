//  User Tabs Data Constant

(function (angular) {
    "use strict";

    var resPortTabsData = {
        properties: {
            id: "properties",
            text: "Properties",
            isActive: false,
            incUrl: "user/assign-product-access/resident-portal/templates/properties-grid.html"
        },

        roles: {
            id: "roles",
            text: "Roles",
            isActive: true,
            incUrl: "user/assign-product-access/resident-portal/templates/roles-grid.html"
        },

        messagingGroups: {
            id: "messagingGroups",
            text: "Messaging Groups",
            isActive: false,
            incUrl: "user/assign-product-access/resident-portal/templates/messaging-groups.html"
        },

        notifications: {
            id: "notifications",
            text: "Notifications",
            isActive: false,
            incUrl: "user/assign-product-access/resident-portal/templates/notifications.html"
        }
    };


    angular
        .module("settings")
        .value("resPortTabsData", resPortTabsData);
})(angular);

//  Deposit Alt Tabs Data Constant

(function (angular) {
    "use strict";

    var depositAlternativeTabsData = {
        properties: {
            id: "properties",
            text: "Properties",
            isActive: false,
            incUrl: "user/assign-product-access/deposit-alternative/templates/properties-grid.html"

        },

        areas: {
            id: "areas",
            text: "Areas",
            isActive: false,
            incUrl: "user/assign-product-access/deposit-alternative/templates/areas-grid.html"

        },

        regions: {
            id: "regions",
            text: "Regions",
            isActive: false,
            incUrl: "user/assign-product-access/deposit-alternative/templates/regions-grid.html"

        },

        roles: {
            id: "roles",
            text: "Roles",
            isActive: true,
            incUrl: "user/assign-product-access/deposit-alternative/templates/roles-grid.html"
        },

        notifications: {
            id: "notifications",
            text: "Notifications",
            isActive: true,
            incUrl: "user/assign-product-access/deposit-alternative/templates/notifications.html"
        }
    };
    
    angular
        .module("settings")
        .value("depositAlternativeTabsData", depositAlternativeTabsData);
})(angular);
//  User Tabs Data Constant

(function (angular) {
    "use strict";

    var docManagementTabsData = {
        roles: {
            id: "roles",
            text: "Roles",
            isActive: true,
            incUrl: "user/assign-product-access/document-management/templates/roles-grid.html"
        },

        departments: {
            id: "departments",
            text: "Departments",
            isActive: false,
            incUrl: "user/assign-product-access/document-management/templates/departments-grid.html"
        },

        properties: {
            id: "properties",
            text: "Properties",
            isActive: false,
            incUrl: "user/assign-product-access/document-management/templates/properties-grid.html"
        },
    };


    angular
        .module("settings")
        .value("docManagementTabsData", docManagementTabsData);
})(angular);

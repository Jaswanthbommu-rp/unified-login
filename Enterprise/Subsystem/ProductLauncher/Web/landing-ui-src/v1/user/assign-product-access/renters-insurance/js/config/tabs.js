//  User Tabs Data Constant

(function (angular) {
    "use strict";

    var rentersInsuranceTabsData = {
        properties: {
            id: "properties",
            text: "Properties",
            isActive: true,
            incUrl: "user/assign-product-access/renters-insurance/templates/properties.html"
        },

        roles: {
            id: "roles",
            text: "Roles",
            isActive: false,
            incUrl: "user/assign-product-access/renters-insurance/templates/roles-grid.html"
        }
    };

    angular
        .module("settings")
        .value("rentersInsuranceTabsData", rentersInsuranceTabsData);
})(angular);

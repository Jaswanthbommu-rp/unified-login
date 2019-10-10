//  BusinessIntelligence Tabs Data Constant

(function (angular) {
    "use strict";

    var businessIntelligenceTabsData = {

        roles: {
            id: "roles",
            text: "Roles",
            isActive: true,
            incUrl: "user/assign-product-access/asset-optimization/templates/roles.html"
        },
        propertyGroups: {
            id: "propertyGroups",
            text: "Property Groups",
            isActive: false,
            incUrl: "user/assign-product-access/asset-optimization/templates/property-group.html"
        },

        properties: {
            id: "properties",
            text: "Properties",
            isActive: false,
            incUrl: "user/assign-product-access/asset-optimization/templates/properties.html"
        }
    };

    angular
        .module("settings")
        .value("businessIntelligenceTabsData", businessIntelligenceTabsData);
})(angular);

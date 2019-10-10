//  Unified Amenitites User Tabs Data Constant

(function (angular) {
    "use strict";

    var unifiedAmenitiesTabsData = {
        properties: {
            id: "properties",
            text: "Properties",
            isActive: false,
            incUrl: "user/assign-product-access/unified-amenities/templates/properties-grid.html"

        },

        roles: {
            id: "roles",
            text: "Roles",
            isActive: true,
            incUrl: "user/assign-product-access/unified-amenities/templates/roles-grid.html"
        }
    };
    
    angular
        .module("settings")
        .value("unifiedAmenitiesTabsData", unifiedAmenitiesTabsData);
})(angular);
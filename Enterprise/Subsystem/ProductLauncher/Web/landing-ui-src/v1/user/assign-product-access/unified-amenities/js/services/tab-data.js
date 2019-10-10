//  Unified Amenities Product Access Tabs Data Service

(function (angular, undefined) {
    "use strict";

    function UATabsData() {
        var data,
            svc = this;

        svc.data = {
            properties: {
                id: "properties",
                text: "Properties",
                isActive: true,
                incUrl: "user/assign-product-access/unified-amenities/templates/properties-grid.html"

            },

            roles: {
                id: "roles",
                text: "Roles",
                isActive: false,
                incUrl: "user/assign-product-access/unified-amenities/templates/roles-grid.html"
            }
        };

        svc.getData = function () {
            return data;
        };

        svc.getTabData = function (tabName) {
            return svc.data[tabName];
        };

        svc.getList = function () {
            return [
                svc.data.properties,
                svc.data.roles
            ];
        };

        svc.reset = function () {
            angular.forEach(svc.data, function (tab, key) {
                tab.isActive = (key === "properties");
            });
        };
    }

    angular
        .module("settings")
        .service("uaTabsData", [
            UATabsData
        ]);
})(angular);

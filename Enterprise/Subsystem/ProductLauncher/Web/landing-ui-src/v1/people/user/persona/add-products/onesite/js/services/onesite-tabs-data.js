//  Onesite Product Access Tabs Data Service

(function(angular, undefined) {
    "use strict";

    function OsTabsData() {
        var data,
            svc = this;

        svc.data = {
            properties: {
                id: "properties",
                text: "Properties",
                isActive: true,
                incUrl: "people/user/persona/add-products/onesite/templates/properties-grid.html"

            },

            roles: {
                id: "roles",
                text: "Roles",
                isActive: false,
                incUrl: "people/user/persona/add-products/onesite/templates/roles-grid.html"
            }
        };

        svc.getData = function() {
            return data;
        };

        svc.getTabData = function(tabName) {
            return svc.data[tabName];
        };

        svc.getList = function() {
            return [
                svc.data.properties,
                svc.data.roles
            ];
        };

        svc.reset = function() {
            angular.forEach(svc.data, function(tab, key) {
                tab.isActive = (key === "properties");
            });
        };
    }

    angular
        .module("settings")
        .service("osTabsData", [
            OsTabsData
        ]);
})(angular);

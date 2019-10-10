//  Business Intelligence Tab Nav Model

(function (angular) {
    "use strict";

    function BusinessIntelligenceTabsNavModel() {
        var data,
            svc = this;

        svc.data = {
            roles: {
                id: "roles",
                text: "Roles",
                isActive: true,
                incUrl: "user/assign-product-access/business-intelligence/templates/roles.html"
            },
            propertyGroups: {
                id: "propertyGroups",
                text: "Property Groups",
                isActive: false,
                incUrl: "user/assign-product-access/business-intelligence/templates/property-group.html"
            },
            properties: {
                id: "properties",
                text: "Properties",
                isActive: false,
                incUrl: "user/assign-product-access/business-intelligence/templates/properties.html"
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
                svc.data.roles,
                svc.data.propertyGroups,
                svc.data.properties
            ];
        };

        svc.reset = function () {
            angular.forEach(svc.data, function (tab, key) {
                tab.isActive = (key === "roles");
            });
        };
    }

    angular
        .module("settings")
        .service('BusinessIntelligenceTabsNavModel', [
            BusinessIntelligenceTabsNavModel
        ]);
})(angular);

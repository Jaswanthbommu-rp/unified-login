//  UserMgmt Product Access Tabs Data Service

(function (angular, undefined) {
    "use strict";

    function PerformanceAnalyticsTabsData() {
        var data,
            svc = this;

        svc.data = {
            roles: {
                id: "roles",
                text: "Roles",
                isActive: true,
                incUrl: "user/assign-product-access/performance-analytics/templates/companies.html"
            },
            propertygroups: {
                id: "propertygroups",
                text: "Property Groups",
                isActive: false,
                incUrl: "user/assign-product-access/performance-analytics/templates/property-group.html"
            },
            properties: {
                id: "properties",
                text: "Properties",
                isActive: false,
                incUrl: "user/assign-product-access/performance-analytics/templates/company-properties.html"
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
                svc.data.propertygroups,
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
        .service("performanceAnalyticsTabsData", [
            PerformanceAnalyticsTabsData
        ]);
})(angular);

//  UserMgmt Product Access Tabs Data Service

(function (angular, undefined) {
    "use strict";

    function InvestmentAnalyticsTabsData() {
        var data,
            svc = this;

        svc.data = {
            roles: {
                id: "roles",
                text: "Roles",
                isActive: true,
                incUrl: "user/assign-product-access/investment-analytics/templates/companies.html"
            },
            markets: {
                id: "markets",
                text: "Markets",
                isActive: false,
                incUrl: "user/assign-product-access/investment-analytics/templates/markets.html"
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
                svc.data.markets
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
        .service("investmentAnalyticsTabsData", [
            InvestmentAnalyticsTabsData
        ]);
})(angular);

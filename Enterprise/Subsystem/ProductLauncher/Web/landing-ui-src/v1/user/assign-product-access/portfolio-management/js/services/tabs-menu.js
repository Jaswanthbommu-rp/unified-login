//  Nav Model

(function (angular) {
    "use strict";

    function PortfolioManagementTabsData() {
        var data,
            svc = this;

        svc.data = {
            entities: {
                id: "entities",
                incUrl: "user/assign-product-access/portfolio-management/templates/entities.html",
                className: "",
                isActive: true,
                text: "Entity Roles"
            },
            roles: {
                id: "roles",
                incUrl: "user/assign-product-access/portfolio-management/templates/global-roles.html",
                className: "",
                isActive: false,
                text: "Global Roles"
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
                svc.data.entities,
                svc.data.roles
            ];
        };

        svc.reset = function() {
            angular.forEach(svc.data, function(tab, key) {
                tab.isActive = (key === "entities");
            });
        };
    }

    angular
        .module("settings")
        .service('PortfolioManagementTabsData', [
            PortfolioManagementTabsData
        ]);
})(angular);

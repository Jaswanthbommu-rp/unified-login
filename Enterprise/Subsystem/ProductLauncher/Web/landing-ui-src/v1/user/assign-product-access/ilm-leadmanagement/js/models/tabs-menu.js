//  Nav Model

(function (angular) {
    "use strict";

    function ILMLeadManagementTabsData() {
        var data,
            svc = this;

        svc.data = {
            properties: {
                id: "properties",
                incUrl: "user/assign-product-access/ilm-leadmanagement/templates/properties.html",
                className: "",
                isActive: true,
                text: "Properties"
            },
            roles: {
                id: "roles",
                incUrl: "user/assign-product-access/ilm-leadmanagement/templates/roles.html",
                className: "",
                isActive: false,
                text: "Roles"
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
                svc.data.roles,
                svc.data.notifications
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
        .service('ILMLeadManagementTabsData', [
            ILMLeadManagementTabsData
        ]);
})(angular);

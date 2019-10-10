//  Utility Management Nav Model

(function (angular) {
    "use strict";

    function UtilityManagementNavModel() {
        var data,
            svc = this;

        svc.data = {
            properties: {
                id: "properties",
                text: "Properties",
                isActive: true,
                incUrl: "user/assign-product-access/utility-management/templates/properties.html"
            },
            roles: {
                id: "roles",
                text: "Additional Rights",
                isActive: false,
                incUrl: "user/assign-product-access/utility-management/templates/roles.html"
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
        .service('UtilityManagementNavModel', [
            UtilityManagementNavModel
        ]);
})(angular);

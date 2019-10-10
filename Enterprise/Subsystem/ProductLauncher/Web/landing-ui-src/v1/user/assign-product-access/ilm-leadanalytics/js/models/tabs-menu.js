//  Nav Model

(function (angular) {
    "use strict";

    function ILMLeadAnalyticsTabsData() {
        var data,
            svc = this;

        svc.data = {
            propertygroups: {
                id: "propertygroups",
                incUrl: "user/assign-product-access/ilm-leadanalytics/templates/property-group.html",
                className: "",
                isActive: true,
                text: "Property Groups"
            },
            properties: {
                id: "properties",
                incUrl: "user/assign-product-access/ilm-leadanalytics/templates/properties.html",
                className: "",
                isActive: false,
                text: "Properties"
            },
            roles: {
                id: "roles",
                incUrl: "user/assign-product-access/ilm-leadanalytics/templates/roles.html",
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
                svc.data.propertygroups,
                svc.data.properties,
                svc.data.roles
            ];
        };

        svc.reset = function() {
            angular.forEach(svc.data, function(tab, key) {
                tab.isActive = (key === "propertygroups");
            });
        };
    }

    angular
        .module("settings")
        .service('ILMLeadAnalyticsTabsData', [
            ILMLeadAnalyticsTabsData
        ]);
})(angular);

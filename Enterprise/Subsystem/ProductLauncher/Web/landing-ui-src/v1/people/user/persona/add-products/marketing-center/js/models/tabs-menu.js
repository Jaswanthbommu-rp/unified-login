//  Product Access Tabs Data Service

(function(angular, undefined) {
    "use strict";

    function MCNavModel() {
        var data,
            svc = this;

        svc.data = {
            propertyGroups: {
                id: "propertyGroups",
                incUrl: "people/user/persona/add-products/marketing-center/templates/property-groups.html",
                className: "",
                isActive: false,
                text: "Property Groups"
            }, 
            properties: {
                id: "properties",
                incUrl: "people/user/persona/add-products/marketing-center/templates/properties.html",
                className: "",
                isActive: true,
                text: "Properties"
            }, 
            roles: {
                id: "roles",
                incUrl: "people/user/persona/add-products/marketing-center/templates/roles.html",
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
                svc.data.propertyGroups,
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
        .service("MCNavModel", [
            MCNavModel
        ]);
})(angular);

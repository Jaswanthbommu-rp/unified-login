//  Nav Model

(function (angular) {
    "use strict";

    function VendCompNavModel() {
        var data,
            svc = this;

        svc.data = {
            properties: {
                id: "properties",
                incUrl: "people/user/persona/add-products/vendor-compliance/templates/properties.html",
                className: "",
                isActive: true,
                text: "Properties"
            }, 
            roles: {
                id: "roles",
                incUrl: "people/user/persona/add-products/vendor-compliance/templates/roles.html",
                className: "",
                isActive: false,
                text: "Roles"
            }, 
            notifications: {
                id: "notifications",
                incUrl: "people/user/persona/add-products/vendor-compliance/templates/notifications.html",
                className: "",
                isActive: false,
                text: "Notifications"
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
        .service('VendCompNavModel', [
            VendCompNavModel
        ]);
})(angular);

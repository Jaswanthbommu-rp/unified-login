//  Spend Management Nav Model

(function (angular) {
    "use strict";

    function SMNavModel() {
        var data,
            svc = this;

        svc.data = {
            propertyGroups: {
                id: "propertyGroups",
                incUrl: "people/user/persona/add-products/spend-management/templates/property-group.html",
                className: "",
                isActive: true,
                text: "Property Groups"
            }, 
            roles: {
                id: "roles",
                incUrl: "people/user/persona/add-products/spend-management/templates/roles.html",
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
                svc.data.roles
            ];
        };

        svc.reset = function() {
            angular.forEach(svc.data, function(tab, key) {
                tab.isActive = (key === "propertyGroups");
            });
        };
    }

    angular
        .module("settings")
        .service('SMNavModel', [
            SMNavModel
        ]);
})(angular);

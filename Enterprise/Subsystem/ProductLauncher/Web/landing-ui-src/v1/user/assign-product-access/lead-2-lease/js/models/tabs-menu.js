//  Product Access Tabs Data Service

(function(angular, undefined) {
    "use strict";

    function Lead2LeaseNavModel() {
        var data,
            svc = this;

        svc.data = {
            properties: {
                id: "properties",
                incUrl: "user/assign-product-access/lead-2-lease/templates/properties.html",
                className: "",
                isActive: true,
                text: "Properties"
            },
            roles: {
                id: "roles",
                incUrl: "user/assign-product-access/lead-2-lease/templates/roles.html",
                className: "",
                isActive: false,
                text: "Rights"
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
        .service("lead2LeaseNavModel", [
            Lead2LeaseNavModel
        ]);
})(angular);

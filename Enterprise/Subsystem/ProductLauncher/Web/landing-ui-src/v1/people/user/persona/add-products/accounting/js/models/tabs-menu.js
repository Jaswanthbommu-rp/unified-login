//  Product Access Tabs Data Service

(function(angular, undefined) {
    "use strict";

    function ANavModel() {
        var data,
            svc = this;

        svc.data = {
            // entityGroups: {
            //     id: "entityGroups",
            //     incUrl: "people/user/persona/add-products/accounting/templates/entity-groups.html",
            //     className: "",
            //     isActive: true,
            //     text: "Entity Groups"
            // },
            entities: {
                id: "entities",
                incUrl: "people/user/persona/add-products/accounting/templates/entities.html",
                className: "",
                isActive: true,
                text: "Entities / Groups"
            }, 
            roles: {
                id: "roles",
                incUrl: "people/user/persona/add-products/accounting/templates/roles.html",
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
        .service("ANavModel", [
            ANavModel
        ]);
})(angular);

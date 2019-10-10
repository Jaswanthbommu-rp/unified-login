//  Product Access Tabs Data Service

(function(angular, undefined) {
    "use strict";

    function ANavModel() {
        var data,
            svc = this;

        svc.data = {
            // entityGroups: {
            //     id: "entityGroups",
            //     incUrl: "user/assign-product-access/accounting/templates/entity-groups.html",
            //     className: "",
            //     isActive: true,
            //     text: "Entity Groups"
            // },
            companies: {
                id: "companies",
                incUrl: "user/assign-product-access/accounting/templates/companies.html",
                className: "",
                isActive: true,
                text: "Companies"
            },      
            entities: {
                id: "entities",
                incUrl: "user/assign-product-access/accounting/templates/entities.html",
                className: "",
                isActive: false,
                text: "Entities"
            },
            roles: {
                id: "roles",
                incUrl: "user/assign-product-access/accounting/templates/roles.html",
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
                svc.data.companies,
                svc.data.entities,
                svc.data.roles,

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

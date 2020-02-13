//   Tabs Data Service

(function (angular) {
    "use strict";

    function TabsData(model) {
        var data,
            svc = this;

        // svc.data = {
        //     properties: {
        //         id: "properties",
        //         incUrl: "user/assign-product-access/new-product-access/templates/properties.html",
        //         className: "",
        //         isActive: true,
        //         text: "Properties"
        //     }, 
        //     roles: {
        //         id: "roles",
        //         incUrl: "user/assign-product-access/new-product-access/templates/roles.html",
        //         className: "",
        //         isActive: false,
        //         text: "Roles"
        //     }
        // };

        svc.data = model.getTabsData();


        svc.setData = function(d) {
            data = d;
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
        .service("tabsData", [
            "newProductAccessModel",
            TabsData
            
        ]);
})(angular);

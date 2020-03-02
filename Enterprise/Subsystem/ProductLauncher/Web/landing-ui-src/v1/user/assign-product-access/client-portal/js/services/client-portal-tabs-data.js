//  Client Portal Product Access Tabs Data Service

(function (angular) {
    "use strict";

    function ClientPortalTabsData(jsonData) {
        var data,
            svc = this;

        // svc.data = {
        //     properties: {
        //         id: "properties",
        //         incUrl: "user/assign-product-access/client-portal/templates/properties.html",
        //         className: "",
        //         isActive: true,
        //         text: "Properties"
        //     },
        //     roles: {
        //         id: "roles",
        //         incUrl: "user/assign-product-access/client-portal/templates/roles.html",
        //         className: "",
        //         isActive: false,
        //         text: "Roles"
        //     }
        // };



        svc.getTabsData = function () {

            var tabs = {}, i=0;
            if(jsonData && jsonData.Tabs){
                jsonData.Tabs.forEach(function (data) {
                    tabs[data.DisplayName.toLowerCase()] = {
                        id : data.DisplayName.toLowerCase(),
                        text : data.DisplayName,
                        isActive :  i === 0 ? true : false,
                        incUrl: "user/assign-product-access/client-portal/templates/" + data.DisplayName.toLowerCase() + ".html",
                    };
                    i++;
                });
            }

            return tabs;
        };


        svc.setData = function(data) {
            svc.data = data;
        };

        svc.getData = function() {
            return data;
        };

        svc.getTabData = function(tabName) {
            return svc.data[tabName];
        };

        svc.getList = function() {
            return [
                // svc.data.properties,
                // svc.data.roles
            ];
        };

        svc.reset = function() {
            angular.forEach(svc.data, function(tab, key) {
                tab.isActive = (key === "properties");
            });
        };

        svc.data = svc.getTabsData();
    }

    angular
        .module("settings")
        .service("clientPortalTabsData", [
            "DataModel",
            ClientPortalTabsData
        ]);
})(angular);

//  UserMgmt Product Access Tabs Data Service

(function(angular, undefined) {
    "use strict";

    function UserMgmtTabsData() {
        var data,
            svc = this;

        svc.data = {           
            roles: {
                id: "roles",
                text: "Roles",
                isActive: true,
                incUrl: "user/assign-product-access/unified-login/templates/roles.html"
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
                svc.data.roles
            ];
        };

        svc.reset = function() {
            angular.forEach(svc.data, function(tab, key) {
                tab.isActive = (key === "roles");
            });
        };
    }

    angular
        .module("settings")
        .service("userMgmtTabsData", [
            UserMgmtTabsData
        ]);
})(angular);

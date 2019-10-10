//  research-application Tabs Data Service

(function(angular, undefined) {
    "use strict";

    function ResAppTabsData() {
        var data,
            svc = this;

        svc.data = {
            roles: {
                id: "roles",
                text: "Roles",
                isActive: true,
                incUrl: "user/assign-product-access/research-application/templates/roles.html"
            },
            // goals: {
            //     id: "goals",
            //     text: "Goals",
            //     isActive: false,
            //     incUrl: "user/assign-product-access/research-application/templates/goals.html"
            // }
        };

       
        svc.reset = function() {
            angular.forEach(svc.data, function(tab, key) {
                tab.isActive = (key === "roles");
            });
        };
    }

    angular
        .module("settings")
        .service("resAppTabsData", [
            ResAppTabsData
        ]);
})(angular);
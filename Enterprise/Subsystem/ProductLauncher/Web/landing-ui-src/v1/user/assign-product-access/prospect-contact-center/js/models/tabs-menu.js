//  Product Access Tabs Data Service

(function(angular, undefined) {
    "use strict";

    function ProspectContactCenterNavModel() {
        var data,
            svc = this;

        svc.data = {
            properties: {
                id: "properties",
                incUrl: "user/assign-product-access/prospect-contact-center/templates/properties.html",
                className: "",
                isActive: true,
                text: "Properties"
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
        .service("prospectContactCenterNavModel", [
            ProspectContactCenterNavModel
        ]);
})(angular);

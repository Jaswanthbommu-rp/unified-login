//  userMgmt RIghts List Aside

(function(angular, undefined) {
    "use strict";

    function factory(asideModal) {
        var modalData = {
            keyboard: false,
            backdrop: "static",
            templateUrl: "user/assign-product-access/portfolio-management/templates/entities-list-aside.html"
        };

        return asideModal().setData(modalData);
    }

    angular
        .module("settings")
        .factory("entitiesListAside", ["rpAsideModal", factory]);
})(angular);
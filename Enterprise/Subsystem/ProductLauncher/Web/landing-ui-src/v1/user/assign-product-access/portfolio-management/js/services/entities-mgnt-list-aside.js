//  Assign Property Aside Service

(function (angular) {
    "use strict";

    function EntitiesRolesAsideSvc(asideModal) {
        var modalData = {
            keyboard: false,
            backdrop: "static",
            templateUrl: "user/assign-product-access/portfolio-management/templates/entities-list-aside.html"
        };

        return asideModal().setData(modalData);
    }

    angular
        .module("settings")
        .factory("entitiesRolesAside", ["rpAsideModal", EntitiesRolesAsideSvc]);
})(angular);
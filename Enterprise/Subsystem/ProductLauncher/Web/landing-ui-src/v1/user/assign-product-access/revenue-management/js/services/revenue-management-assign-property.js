//  Assign Property Aside Service

(function (angular, undefined) {
    "use strict";

    function factory(asideModal) {
        var modalData = {
            keyboard: false,
            backdrop: "static",
            templateUrl: "user/assign-product-access/revenue-management/templates/properties-assigned-list-aside.html"
        };

        return asideModal().setData(modalData);
    }

    angular
        .module("settings")
        .factory("rmAssignPropertyAside", ["rpAsideModal", factory]);
})(angular);

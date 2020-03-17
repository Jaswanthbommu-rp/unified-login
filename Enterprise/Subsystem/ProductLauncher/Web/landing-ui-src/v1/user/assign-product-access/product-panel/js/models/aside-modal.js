//   Aside Modal

(function(angular, undefined) {
    "use strict";

    function factory(asideModal) {
        var modalData = {
            keyboard: false,
            backdrop: "static",
            templateUrl: "user/assign-product-access/product-panel/templates/aside.html"
        };

        return asideModal().setData(modalData);
    }

    angular
        .module("settings")
        .factory("asideModal", ["rpAsideModal", factory]);
})(angular);
//  Sites List Aside

(function (angular, undefined) {
    "use strict";

    function factory(asideModal) {
        var modalData = {
            keyboard: false,
            backdrop: "static",
            templateUrl: "user/assign-product-access/click-pay/templates/sites-aside.html"
        };

        return asideModal().setData(modalData);
    }

    angular
        .module("settings")
        .factory("cpSiteListAside", ["rpAsideModal", factory]);
})(angular);

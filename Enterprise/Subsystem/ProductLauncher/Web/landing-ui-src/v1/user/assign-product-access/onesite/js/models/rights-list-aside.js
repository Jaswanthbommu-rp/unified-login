//  RIghts List Aside

(function (angular, undefined) {
    "use strict";

    function factory(asideModal) {
        var modalData = {
            keyboard: false,
            backdrop: "static",
            templateUrl: "user/assign-product-access/onesite/templates/rights-list-aside.html"
    };

        return asideModal().setData(modalData);
    }

    angular
        .module("settings")
        .factory("osRightsListAside", ["rpAsideModal", factory]);
})(angular);

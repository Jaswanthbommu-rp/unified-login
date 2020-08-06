//  RIghts List Aside

(function (angular, undefined) {
    "use strict";

    function factory(asideInnerModal) {
        var modalData = {
            keyboard: false,
            backdrop: "static",
            templateUrl: "user/assign-product-access/product-panel/templates/product-panel-list-aside.html"
    };

        return asideInnerModal().setData(modalData);
    }

    angular
        .module("settings")
        .factory("productPanelListInnerAside", ["rpAsideModal", factory]);
})(angular);

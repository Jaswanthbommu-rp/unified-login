//  RIghts List Aside

(function (angular, undefined) {
    "use strict";

    function factory(asideModal) {
        var modalData = {
            keyboard: false,
            backdrop: "static",
            templateUrl: "user/assign-product-access/product-panel/templates/product-panel-list-aside.html"
    };

        return asideModal().setData(modalData);
    }

    angular
        .module("settings")
        .factory("productPanelListInnerAside", ["rpAsideModal", factory]);
})(angular);

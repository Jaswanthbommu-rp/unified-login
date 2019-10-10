//  Accounting Entity Group Details Aside

(function (angular, undefined) {
    "use strict";

    function factory(asideModal) {
        var modalData = {
            keyboard: false,
            backdrop: "static",
            templateUrl: "user/assign-product-access/accounting/templates/entity-group-details-aside.html"
        };

        return asideModal().setData(modalData);
    }

    angular
        .module("settings")
        .factory("AEntityGroupDetailsAside", ["rpAsideModal", factory]);
})(angular);

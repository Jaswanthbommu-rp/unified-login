//  Marketing Center Property Group Details Aside

(function (angular, undefined) {
    "use strict";

    function factory(asideModal) {
        var modalData = {
            keyboard: false,
            backdrop: "static",
            templateUrl: "user/assign-product-access/marketing-center/templates/property-group-details-aside.html"
        };

        return asideModal().setData(modalData);
    }

    angular
        .module("settings")
        .factory("MCPropertyGroupDetailsAside", [
            "rpAsideModal",
            factory
        ]);
})(angular);

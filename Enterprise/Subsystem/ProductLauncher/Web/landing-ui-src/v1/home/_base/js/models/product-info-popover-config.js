//  Product Information Model

(function (angular, undefined) {
    "use strict";

    function factory(popoverConfig) {
        return function (data) {
            return popoverConfig({
                modelData: data,
                autoClose: true,
                trigger: "click",
                placement: "bottom",
                instName: "productInfoPopover",
                container: "body",
                templateUrl: "home/base/templates/product-more-info.html"
            });
        };
    }

    angular
        .module("settings")
        .factory("productInfoPopoverConfig", ["rpPopoverConfig", factory]);
})(angular);

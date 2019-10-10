//  Products Carousel Product Directive

(function (angular, undefined) {
    "use strict";

    function productsCarouselProduct() {
        function link(scope, elem, attr) {
            var dir = {};

            dir.init = function () {
                var model = scope.$eval(attr.productsCarouselProduct);
                if (scope.productCtrl) {
                    scope.productCtrl.setModel(model);
                }
                else {
                    logc("couldn't find controller");
                }
                dir.destWatch = scope.$on("$destroy", dir.destroy);
            };

            dir.destroy = function () {
                dir.destWatch();
                dir = undefined;
                attr = undefined;
                elem = undefined;
                scope = undefined;
            };

            dir.init();
        }

        return {
            link: link,
            restrict: "A"
        };
    }

    angular
        .module("settings")
        .directive("productsCarouselProduct", [
            productsCarouselProduct
        ]);
})(angular);

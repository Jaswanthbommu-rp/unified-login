//  Products Carousel Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function ProductsCarousel() {
            var s = this;
            s.data = {};
            s.init();
        }

        var p = ProductsCarousel.prototype;

        p.init = function () {
            var s = this;
        };

        p.getData = function () {
            var s = this;
            return s.data;
        };

        p.setData = function (productsData) {
            var s = this,
                list = [];
            if (productsData && productsData.length > 0) {
                var pair = [];

                productsData.forEach(function (item) {
                    pair.push(item);

                    if (pair.length == 2) {
                        list.push({
                            list: pair
                        });
                        pair = [];
                    }
                });

                if (pair.length % 2 !== 0) {
                    list.push({
                        list: pair
                    });
                }
            }

            s.data = list;
        };

        p.reset = function () {
            var s = this;
            s.data = {};
        };

        return new ProductsCarousel();
    }

    angular
        .module("settings")
        .factory("productsCarouselModel", [
            factory
        ]);
})(angular);

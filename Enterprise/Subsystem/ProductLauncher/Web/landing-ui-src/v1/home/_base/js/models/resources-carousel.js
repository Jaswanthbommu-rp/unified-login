//  Resources Carousel Model

(function (angular, undefined) {
    "use strict";

    function factory(resourceCardModel) {
        function ResourcesCarousel() {
            var s = this;
            s.data = [];
            s.init();
        }

        var p = ResourcesCarousel.prototype;

        p.init = function () {
            var s = this;
        };

        p.getData = function () {
            var s = this;
            return s.data;
        };

        p.setData = function (resourcesData) {
            var s = this;
            s.data = resourcesData;
        };

        p.reset = function () {
            var s = this;
            s.data = [];
        };

        return new ResourcesCarousel();
    }

    angular
        .module("settings")
        .factory("resourcesCarouselModel", [
            "resourceCardModel",
            factory
        ]);
})(angular);

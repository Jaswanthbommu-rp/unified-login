//  Home Controller

(function (angular, undefined) {
    "use strict";

    function HomeCtrl($scope, userSummarySvc, userProfileModel, externalLinks, dashboard, pubsub, carouselModel, productsCarouselModel, resourcesCarouselModel) {
        var vm = this;

        vm.init = function () {
            vm.showResourcesList = true;
            vm.productsCarousel = carouselModel();
            vm.resourcesCarousel = carouselModel();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.userProfileWatch = userProfileModel.subscribe(vm.userProfileUpdated);

            vm.isError = false;
            vm.errorMessage = null;

            vm.model = null;
            vm.realpageMainUrl = externalLinks.realpageMain;

            vm.isLoading = true;

            if (dashboard.isReady()) {
                vm.processData(dashboard.getData());
            }
            else {
                dashboard.load();
            }

            vm.unsubDash = dashboard.subscribe(vm.processData);
            vm.favChangeWatch = pubsub.subscribe("prodSolnFav.change", vm.onFavChange);
        };

        vm.onFavChange = function () {
            dashboard.reload();
        };

        vm.logoutUser = function () {
            pubsub.publish("signout.rpGlobalHeader");
        };

        vm.userProfileUpdated = function () {
            dashboard.reload();
        };

        vm.processData = function (rawData) {
            var processedData = userSummarySvc.formatResponse(rawData);
            vm.setDashboardData(processedData);
        };

        vm.setDashboardData = function (response) {
            vm.model = response.dashboard;
            vm.setupCarousels(vm.model);
        };

        vm.setupCarousels = function (data) {
            if (data.resources && data.resources.length === 0) {
                vm.showResourcesList = false;
            }
            productsCarouselModel.setData(data.productSummary);
            vm.setProductsCarouselData();
            resourcesCarouselModel.setData(data.resources);
            vm.setResourcesCarouselData();
            vm.isLoading = false;
        };

        vm.setProductsCarouselData = function () {
            var productsCarouselData = {
                unitWidth: 280,
                unitHeight: 220,
                templateUrl: "home/base/templates/product-card.html"
            };

            productsCarouselData.units = productsCarouselModel.getData();
            vm.productsCarousel.setData(productsCarouselData);
        };

        vm.setResourcesCarouselData = function () {
            var resourcesCarouselData = {
                unitWidth: 174,
                unitHeight: 104,
                templateUrl: "home/base/templates/resource-card.html"
            };
            resourcesCarouselData.units = resourcesCarouselModel.getData();
            vm.resourcesCarousel.setData(resourcesCarouselData);
        };

        vm.setError = function (reason) {
            vm.isLoading = false;
            vm.isError = true;
            vm.errorMessage = reason;
        };

        vm.isProductDisabled = function (product) {
            return product.productStatus !== 8;
        };

        vm.destroy = function () {
            vm.unsubDash();

            vm.destWatch();
            vm.destWatch = undefined;

            vm.userProfileWatch();
            vm.userProfileWatch = undefined;

            vm.model = undefined;
            vm = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("HomeCtrl", [
            "$scope",
            "userSummarySvc",
            "userProfileModel",
            "externalLinks",
            "dashboardModel",
            "pubsub",
            "rpCarouselModel",
            "productsCarouselModel",
            "resourcesCarouselModel",
            HomeCtrl
        ]);
})(angular);

//  ProductsCarouselProductCtrl Controller

(function (angular, undefined) {
    "use strict";

    function ProductsCarouselProductCtrl($scope, $location, $window, popoverConfig, productSolutionModel, modalModel, elmsSvc) {
//
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.modelData = {};
            vm.productInfoPopoverConfig = popoverConfig(vm.modelData);
            vm.modal = modalModel("access-easy-lms/base/templates/access-easy-lms-modal.html");
        };

        vm.setModel = function (modelData) {
            vm.model = productSolutionModel(modelData);
            angular.extend(vm.modelData, modelData);
        };

        vm.getProductId = function () {
            return "prod" + vm.model.data.productId;
        };

        vm.showELMSModal = function () {
            vm.getELMSUrl();
        };

        vm.getELMSUrl = function() {
            elmsSvc.getELMSUrl().then(vm.verifyELMSResponse);
        };

        vm.verifyELMSResponse = function (response) {
            if (response.isError) {
                if (response.errorCode === "ProductEasyLMS.PELMSUrl.6" || response.errorCode === "ProductEasyLMS.PELMSUrl.8") {
                    vm.modal.show();
                }
            }
            else {
                $window.open(response.data.url, "plptab");
            }
        };

        vm.destroy = function () {
            vm.destWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProductsCarouselProductCtrl", [
            "$scope",
            "$location",
            "$window",
            "productInfoPopoverConfig",
            "productSolutionModel",
            "rpModalModel",
            "accessEasyLMSSvc",
            ProductsCarouselProductCtrl
        ]);
})(angular);

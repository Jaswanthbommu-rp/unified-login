//  ResourcesCarouselCtrl Controller

(function (angular, undefined) {
    "use strict";

    function ResourcesCarouselCtrl($scope, $window, linkSvc, modalModel, alpSvc, resourceCardModel) {
        var vm = this;

        vm.init = function () {
            vm.model = resourceCardModel($scope.$ctrl.model.data);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.modelData = {};
            vm.modal = modalModel("access-learning-portal/base/templates/access-learning-portal-modal.html");
            vm.model.setSrc(vm);
        };

        vm.invokeLink = function () {
            if (vm.model.data.hasAccess) {
                linkSvc.invoke(vm.model.data);
            }
        };

        vm.showPLPModal = function () {
            vm.getPLPUrl();
        };

        vm.getPLPUrl = function () {
            alpSvc.getPLPUrl().then(vm.verifyPLPResponse);
        };

        vm.verifyPLPResponse = function (response) {
            if (response.isError) {
                if (response.errorCode === "ProductLearningPortal.PLPUrl.3" || response.errorCode === "ProductLearningPortal.PLPUrl.5") {
                    vm.modal.show();
                }
            }
            else {
                $window.open(response.data.url, "plptab");
            }
        };

        vm.getProductId = function () {
            return "res" + vm.model.data.productId;
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
        .controller("ResourcesCarouselCtrl", [
            "$scope",
            "$window",
            "rpInvokeLink",
            "rpModalModel",
            "accessLearningPortalSvc",
            "resourceCardModel",
            ResourcesCarouselCtrl
        ]);
})(angular);

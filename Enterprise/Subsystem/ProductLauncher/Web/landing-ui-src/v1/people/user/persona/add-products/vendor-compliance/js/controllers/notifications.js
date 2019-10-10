//  Notifications Controller

(function (angular, undefined) {
    "use strict";

    function VendCompNotificationsCtrl($scope, VendCompDataModel, switchConfig) {
        var vm = this;

        vm.init = function () {
            vm.insurance = false;
            vm.recommendation = false;
            vm.insuranceSwitch = switchConfig({
                onChange: vm.switchInsurance
            });
            vm.recommendationSwitch = switchConfig({
                onChange: vm.switchRecommendation
            });

            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.switchInsurance = function (val) {
            VendCompDataModel.setInsuranceExpired(val);
        };

        vm.switchRecommendation = function (val) {
            VendCompDataModel.setIsVendorChangeRec(val);
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
        .controller("VendCompNotificationsCtrl", [
            "$scope",
            "VendorComplianceDataModel",
            "rpSwitchConfig",
            VendCompNotificationsCtrl]);
})(angular);

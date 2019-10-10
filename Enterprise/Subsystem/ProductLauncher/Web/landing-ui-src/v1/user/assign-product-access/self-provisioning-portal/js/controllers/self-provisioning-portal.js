// Self-Provisioning Portal Controller

(function (angular, undefined) {
    "use strict";

    function SelfProvisioningPortalProductAccessCtrl($scope, $filter, model) {
        var vm = this;

        vm.init = function () {
            vm.panelName = $filter("productPanelText")("panelName.selfProvisioningPortal");
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.isActive = function () {
            return model.isActive();
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
        .controller("SelfProvisioningPortalProductAccessCtrl", [
            "$scope",
            "$filter",
            "selfProvisioningPortalProductAccessModel",
            SelfProvisioningPortalProductAccessCtrl]);
})(angular);

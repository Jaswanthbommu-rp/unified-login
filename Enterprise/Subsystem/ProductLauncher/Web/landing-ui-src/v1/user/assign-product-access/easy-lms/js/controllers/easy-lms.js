//  Coming Soon Controller

(function (angular, undefined) {
    "use strict";

    function EasyLMSProductAccessCtrl($scope, $filter, model) {
        var vm = this;

        vm.init = function () {
            vm.panelName = $filter("productPanelText")("panelName.easylms");
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
        .controller("EasyLMSProductAccessCtrl", [
            "$scope",
            "$filter",
            "easyLMSProductAccessModel",
            EasyLMSProductAccessCtrl]);
})(angular);

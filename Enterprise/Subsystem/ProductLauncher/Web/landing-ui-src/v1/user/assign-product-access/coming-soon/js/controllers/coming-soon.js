//  Coming Soon Controller

(function (angular, undefined) {
    "use strict";

    function ComingSoonProductAccessCtrl($scope, $filter, model) {
        var vm = this;

        vm.init = function () {
            vm.panelName = $filter("productPanelText")("panelName.comingSoon");
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
        .controller("ComingSoonProductAccessCtrl", [
            "$scope",
            "$filter",
            "comingSoonProductAccessModel",
            ComingSoonProductAccessCtrl]);
})(angular);

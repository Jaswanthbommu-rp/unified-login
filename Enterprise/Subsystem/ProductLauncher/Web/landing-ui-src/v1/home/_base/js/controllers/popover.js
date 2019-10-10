//  ProductModelPopoverCtrl Controller

(function (angular, undefined) {
    "use strict";

    function ProductModelPopoverCtrl($scope, externalLinks) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.realpageMainUrl = externalLinks.realpageMain;
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
        .controller("ProductModelPopoverCtrl", [
            "$scope",
            "externalLinks",
            ProductModelPopoverCtrl
        ]);
})(angular);

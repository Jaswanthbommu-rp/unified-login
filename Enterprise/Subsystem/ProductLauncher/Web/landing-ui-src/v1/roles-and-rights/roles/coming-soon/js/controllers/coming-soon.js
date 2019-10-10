//  Coming Soon Controller

(function (angular, undefined) {
    "use strict";

    function ComingSoonRightsCtrl($scope, model) {
        var vm = this;

        vm.init = function () {
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
        .controller("ComingSoonRightsCtrl", ["$scope", "comingSoonRightsModel", ComingSoonRightsCtrl]);
})(angular);

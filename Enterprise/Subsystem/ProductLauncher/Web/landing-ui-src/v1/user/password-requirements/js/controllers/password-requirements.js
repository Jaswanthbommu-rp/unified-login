//  Password Requirements Controller

(function (angular, undefined) {
    "use strict";

    function PassReqCtrl($scope, model) {
        var vm = this;

        vm.init = function () {
            vm.model = model;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
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
        .controller("PassReqCtrl", ["$scope", "passReqModel", PassReqCtrl]);
})(angular);

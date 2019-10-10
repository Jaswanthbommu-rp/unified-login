// Change Admin to Regular User Modal Controller

(function (angular, undefined) {
    "use strict";

    function AddUserSuccessModalCtrl($scope, $location, modal) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.dismissModal = function () {
            modal.hide();
            $location.path("/people/users");
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
        .controller("AddUserSuccessModalCtrl", [
            "$scope",
            "$location",
            "chgAdminToRegularModal",
            AddUserSuccessModalCtrl
        ]);
})(angular);

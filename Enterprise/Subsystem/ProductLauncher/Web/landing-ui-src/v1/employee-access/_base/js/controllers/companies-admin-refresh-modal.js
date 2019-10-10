// 

(function (angular, undefined) {
    "use strict";

    function CompAdminModalCtrl($scope, $location, modal) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.dismissModal = function () {
            modal.hide();           
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
        .controller("CompAdminModalCtrl", [
            "$scope",
            "$location",
            "compAdminRefreshMsgModal",
            CompAdminModalCtrl
        ]);
})(angular);

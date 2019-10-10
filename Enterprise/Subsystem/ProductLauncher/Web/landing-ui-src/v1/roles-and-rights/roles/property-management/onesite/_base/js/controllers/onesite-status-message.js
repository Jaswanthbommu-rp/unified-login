//  Onesite Status Message Controller

(function (angular, undefined) {
    "use strict";

    function OnesiteStatusMsgCtrl($scope, $location, model, modal) {
        var vm = this;

        vm.init = function () {
            vm.model = model;
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
        .controller("OnesiteStatusMsgCtrl", [
            "$scope",
            "$location",
            "onesiteStatusMsgModel",
            "onesiteStatusMsgModal",
            OnesiteStatusMsgCtrl
        ]);
})(angular);

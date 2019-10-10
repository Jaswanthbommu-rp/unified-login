//  spend mgmt Status Message Controller

(function (angular, undefined) {
    "use strict";

    function SpndMgmtStatusMsgCtrl($scope, $location, model, modal) {
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
        .controller("SpndMgmtStatusMsgCtrl", [
            "$scope",
            "$location",
            "spndMgmtStatusMsgModel",
            "spndmgmtStatusMsgModal",
            SpndMgmtStatusMsgCtrl
        ]);
})(angular);

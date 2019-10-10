//  Deposit Alt Roles Radio Controller

(function (angular, undefined) {
    "use strict";

    function DepositAltRolesRadioCtrl($scope, pubsub) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.publishRoleChangeDa = function (record) {            
            pubsub.publish("da.roles-radio", record);
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
        .controller("DepositAltRolesRadioCtrl", [
            "$scope",
            "pubsub",
            DepositAltRolesRadioCtrl]);
})(angular);

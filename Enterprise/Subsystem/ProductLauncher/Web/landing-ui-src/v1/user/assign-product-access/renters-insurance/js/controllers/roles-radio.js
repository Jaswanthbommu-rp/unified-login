//  Renters Insurance Roles Radio Controller

(function (angular, undefined) {
    "use strict";

    function RentersInsuranceRolesRadioCtrl($scope, pubsub) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.publishRoleChange = function (record) {
            pubsub.publish("ri.roles-radio", record);
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
        .controller("RentersInsuranceRolesRadioCtrl", [
            "$scope",
            "pubsub",
            RentersInsuranceRolesRadioCtrl]);
})(angular);

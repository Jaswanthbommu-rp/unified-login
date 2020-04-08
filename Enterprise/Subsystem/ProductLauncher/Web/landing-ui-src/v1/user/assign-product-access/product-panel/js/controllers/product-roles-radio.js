//  Client Portal Properties Radio Controller

(function (angular, undefined) {
    "use strict";

    function ProductPanelRoleRadioCtrl($scope, pubsub) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.publishRoleChange = function (record) {
            pubsub.publish("ppanel.role-radio", record);
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
        .controller("ProductPanelRoleRadioCtrl", [
            "$scope",
            "pubsub",
            ProductPanelRoleRadioCtrl]);
})(angular);

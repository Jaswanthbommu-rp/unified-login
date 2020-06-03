//  Client Portal Properties Radio Controller

(function (angular, undefined) {
    "use strict";

    function ProductPanelPropertyGroupRadioCtrl($scope, pubsub) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.publishPropertyGroupChange = function (record) {
            pubsub.publish("ppanel.property-group-radio", record);
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
        .controller("ProductPanelPropertyGroupRadioCtrl", [
            "$scope",
            "pubsub",
            ProductPanelPropertyGroupRadioCtrl]);
})(angular);

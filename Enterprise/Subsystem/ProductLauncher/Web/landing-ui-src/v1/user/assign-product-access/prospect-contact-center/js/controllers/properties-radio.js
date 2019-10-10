//  Prospect Contact Center Properties Radio Controller

(function (angular, undefined) {
    "use strict";

    function ProspectContactCenterPropertiesRadioCtrl($scope, pubsub) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.publishPropertyChange = function (record) {
            pubsub.publish("pcc.properties-radio", record);
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
        .controller("ProspectContactCenterPropertiesRadioCtrl", [
            "$scope",
            "pubsub",
            ProspectContactCenterPropertiesRadioCtrl]);
})(angular);

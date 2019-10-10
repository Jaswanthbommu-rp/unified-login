//  Property Group Radio Controller

(function (angular, undefined) {
    "use strict";

    function VendCompPropertyGroupRadioCtrl($scope, pubsub) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.publishPropertyChange = function (record) {
        	pubsub.publish("vc.property-group-radio", record);
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
        .controller("VendCompPropertyGroupRadioCtrl", [
        	"$scope",
            "pubsub", 
        	VendCompPropertyGroupRadioCtrl]);
})(angular);

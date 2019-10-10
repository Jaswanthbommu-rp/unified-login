//  Marketing Center Roles Radio Controller

(function (angular, undefined) {
    "use strict";

    function MCRolesRadioCtrl($scope, pubsub) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.publishRoleChange = function (record) {
        	pubsub.publish("mc.roles-radio", record);
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
        .controller("MCRolesRadioCtrl", [
        	"$scope",
            "pubsub",
        	MCRolesRadioCtrl]);
})(angular);

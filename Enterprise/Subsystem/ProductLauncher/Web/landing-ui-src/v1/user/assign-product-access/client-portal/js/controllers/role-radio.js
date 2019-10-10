//  Cilent Portal Roles Radio Controller

(function (angular, undefined) {
    "use strict";

    function ClientPortalRolesRadioCtrl($scope, pubsub) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.publishRoleChange = function (record) {
        	pubsub.publish("cp.roles-radio", record);
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
        .controller("ClientPortalRolesRadioCtrl", [
        	"$scope",
            "pubsub",
        	ClientPortalRolesRadioCtrl]);
})(angular);

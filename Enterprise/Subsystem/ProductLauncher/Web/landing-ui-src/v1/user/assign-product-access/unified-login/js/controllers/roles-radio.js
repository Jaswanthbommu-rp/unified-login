//  UserMgmt Roles Radio Controller

(function(angular, undefined) {
    "use strict";

    function UserMgmtRolesRadioCtrl($scope, pubsub) {
        var vm = this;

        vm.init = function() {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.publishRoleChange = function(record) {
            pubsub.publish("um.roles-radio", record);
        };

        vm.destroy = function() {
            vm.destWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("UserMgmtRolesRadioCtrl", [
            "$scope",
            "pubsub",
            UserMgmtRolesRadioCtrl
        ]);
})(angular);
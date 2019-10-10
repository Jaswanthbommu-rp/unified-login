//  ResApp Roles Radio Controller

(function(angular, undefined) {
    "use strict";

    function ResAppRolesRadioCtrl($scope, pubsub) {
        var vm = this;

        vm.init = function() {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.publishRoleChange = function(record) {
            pubsub.publish("ra.roles-radio", record);
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
        .controller("ResAppRolesRadioCtrl", [
            "$scope",
            "pubsub",
            ResAppRolesRadioCtrl
        ]);
})(angular);
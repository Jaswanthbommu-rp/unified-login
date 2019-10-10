//  IntegMkt Roles Radio Controller

(function(angular, undefined) {
    "use strict";

    function IntegMktRolesRadioCtrl($scope, pubsub) {
        var vm = this;

        vm.init = function() {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.publishRoleChange = function(record) {
            pubsub.publish("intMkt.roles-radio", record);
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
        .controller("IntegMktRolesRadioCtrl", [
            "$scope",
            "pubsub",
            IntegMktRolesRadioCtrl
        ]);
})(angular);
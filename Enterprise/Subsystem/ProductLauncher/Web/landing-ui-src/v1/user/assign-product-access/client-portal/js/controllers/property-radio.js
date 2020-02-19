//  Client Portal Properties Radio Controller

(function (angular, undefined) {
    "use strict";

    function ClientPortalPropertyRadioCtrl($scope, pubsub) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.publishPropertyChange = function (record) {
            if(record.radname == "property"){
                pubsub.publish("cp.property-radio", record);
            }
            else if(record.radname == "role"){
                pubsub.publish("cp.roles-radio", record);
            }
        	
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
        .controller("ClientPortalPropertyRadioCtrl", [
        	"$scope",
            "pubsub",
        	ClientPortalPropertyRadioCtrl]);
})(angular);

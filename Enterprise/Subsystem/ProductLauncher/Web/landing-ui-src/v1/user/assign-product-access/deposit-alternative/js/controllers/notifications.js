//  Notifications Controller

(function (angular, undefined) {
    "use strict";

    function DepositAltNotificationsCtrl($scope, DADataModel, notificationsSvc, persona, pubsub, userDetailsModel, security, switchConfig) {
        var vm = this;

        vm.init = function () {
            
            vm.DADataModel = DADataModel;

            vm.personaWatch = angular.noop;

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }
            
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageDepositAlternativeProductAccess;
        };

       
        vm.loadData = function () {
            if (persona.isReady() ) {                
                vm.noteSwitch = switchConfig({
                    disabled: security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()
                });

                                
                vm.personaWatch();
                
            }
        };

        vm.destroy = function () {
            vm.destWatch();            
            vm.personaWatch();            
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("DepositAltNotificationsCtrl", [
            "$scope",
            "depositAlternativeProductAccessModel",
            "daNotificationsSvc",
            "personaDetails",
            "pubsub",
            "userDetailsModel",
            "routeSecurity",
            "rpSwitchConfig",
            DepositAltNotificationsCtrl]);
})(angular);

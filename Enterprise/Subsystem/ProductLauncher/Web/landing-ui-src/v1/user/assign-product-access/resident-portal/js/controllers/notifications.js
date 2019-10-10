//  Resident Portals Notifications Controller

(function(angular, undefined) {
    "use strict";

    function ResPortNotificationsCtrl($scope, resPortDataModel, resPortNotificationsSvc, persona, pubsub, userDetailsModel, security, switchConfig) {
        var vm = this;

        vm.init = function() {
            vm.frontDesk = false;
            vm.amenity = false;
            vm.serviceReq = false;
            vm.resPortDataModel = resPortDataModel;

            vm.personaWatch = angular.noop;

            if (persona.isReady()) {
                vm.loadData();
            } else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }

            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);
        };

        vm.isActive = function() {
            return resPortDataModel.isActive();
        };

        vm.loadData = function() {
            if (persona.isReady() && vm.isActive()) {
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId()
                };

                vm.activeWatch();
                vm.personaWatch();

                vm.setSwitchStatus();

                vm.dataReq = resPortNotificationsSvc.get(params, vm.setData);
            }
        };

        vm.setSwitchStatus = function() {
            vm.frontDeskSwitch = switchConfig({
                disabled: security.isAllowed("viewUser") || !persona.data.hasResidentPortalUserAccess
            });

            vm.amenitySwitch = switchConfig({
                disabled: security.isAllowed("viewUser") || !persona.data.hasResidentPortalUserAccess
            });

            vm.serviceReqSwitch = switchConfig({
                disabled: security.isAllowed("viewUser") || !persona.data.hasResidentPortalUserAccess
            });
        };

        vm.setData = function(resp) {
            if (resp.data) {
                resPortDataModel.setFrontDesk(resp.data.managerFdiViaEmail);
                resPortDataModel.setAmenity(resp.data.amenitiesViaEmail);
                resPortDataModel.setServiceReq(resp.data.managerMrViaEmail);
            }
        };

        vm.destroy = function() {
            vm.destWatch();
            vm.activeWatch();
            vm.personaWatch();
            resPortDataModel.reset();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ResPortNotificationsCtrl", [
            "$scope",
            "residentPortalsDataModel",
            "resPortNotificationsSvc",
            "personaDetails",
            "pubsub",
            "userDetailsModel",
            "routeSecurity",
            "rpSwitchConfig",
            ResPortNotificationsCtrl
        ]);
})(angular);
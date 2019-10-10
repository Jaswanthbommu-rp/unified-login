//  Notifications Controller

(function (angular, undefined) {
    "use strict";

    function VendCompNotificationsCtrl($scope, vendCompDataModel, vendCompNotificationsSvc, persona, pubsub, userDetailsModel, security, switchConfig) {
        var vm = this;

        vm.init = function () {
            vm.insurance = false;
            vm.recommendation = false;
            vm.vendCompDataModel = vendCompDataModel;

            vm.personaWatch = angular.noop;

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }            

            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);
        };

        vm.loadToggles = function () {
            vm.vendorSwitch = switchConfig({
                disabled: security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()
            });
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageVendorComplianceProductAccess;
        };

        vm.isActive = function () {
            return vendCompDataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && vm.isActive()) {
                vm.loadToggles();
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId()
                };

                vm.activeWatch();
                vm.personaWatch();

                vm.dataReq = vendCompNotificationsSvc.get(params, vm.setData);
            }
        };

        vm.setData = function (resp) {
            vendCompDataModel.setInsuranceExpired(resp.isInsuranceExpired);
            vendCompDataModel.setIsVendorChangeRec(resp.isVendorRecommendationChanges);
            vendCompDataModel.setIsVendorNotLinkedToAnyProperty(resp.isVendorNotLinkedToAnyProperty);
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.activeWatch();
            vm.personaWatch();
            vendCompDataModel.reset();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("VendCompNotificationsCtrl", [
            "$scope",
            "vendorComplianceDataModel",
            "vendCompNotificationsSvc",
            "personaDetails",
            "pubsub",
            "userDetailsModel",
            "routeSecurity",
            "rpSwitchConfig",
            VendCompNotificationsCtrl]);
})(angular);

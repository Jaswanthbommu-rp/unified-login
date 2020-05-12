//  Property Groups Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProductNotificationGridCtrl($scope, $filter, dataSvc, security, persona, syncMgr, productDataModel, userDetailsModel, switchConfig) {
        var vm = this;

        vm.init = function () {
            vm.frontDesk = false;
            vm.amenity = false;
            vm.serviceReq = false;

            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.isActive = function () {
            return productDataModel.isPropertyGridActive();
        };

        vm.hasViewOnlyAccess = function () {
            return security.isAllowed("viewUser") || syncMgr.isUserHasManageProductAccess($scope.$parent.productId);
        };

        vm.loadData = function () {
            var productId = $scope.$parent.productId;
            vm.setSwitchStatus();
            if (persona.isReady() && vm.isActive()) {
                var notificationsData = syncMgr.getProductNotificationsData(productId);
                if (notificationsData === undefined) {
                    var params = {
                        userPersonaId: userDetailsModel.getPersonaId(),
                        editorPersonaId: persona.getId()
                    };
                    vm.datareq = dataSvc.get(params, vm.setNotificationsData);
                }
            }
        };

        vm.setNotificationsData = function(notificationData){
            logc(notificationData.data);
            var productId = $scope.$parent.productId;
            syncMgr.updateProductAllNotifications(productId, notificationData.data);
        };

        vm.setSwitchStatus = function(notificationsData) {
            vm.frontDeskSwitch = switchConfig({
                disabled: security.isAllowed("viewUser") || !persona.data.hasResidentPortalUserAccess,
                trueValue: true
            });

            vm.amenitySwitch = switchConfig({
                disabled: security.isAllowed("viewUser") || !persona.data.hasResidentPortalUserAccess,
                trueValue: true
            });

            vm.serviceReqSwitch = switchConfig({
                disabled: security.isAllowed("viewUser") || !persona.data.hasResidentPortalUserAccess,
                trueValue: true
            });
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.activeWatch();
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProductNotificationGridCtrl", [
            "$scope",
            "$filter",
            "resPortNotificationsSvc",
            "routeSecurity",
            "personaDetails",
            "productDataSyncManager",
            "productPanelDataModel",
            "userDetailsModel",
            "rpSwitchConfig",
            ProductNotificationGridCtrl
        ]);
})(angular);
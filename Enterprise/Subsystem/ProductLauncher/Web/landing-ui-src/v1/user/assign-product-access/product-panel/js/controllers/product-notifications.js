(function (angular, undefined) {
    "use strict";

    function ProductNotificationGridCtrl($scope, $filter, dataSvc, notificationSvc, security, persona, syncMgr, productDataModel, userDetailsModel, switchConfig, pubsub) {
        var vm = this;

        vm.init = function () {
            vm.frontDesk = false;
            vm.amenity = false;
            vm.serviceReq = false;
            vm.isInsuranceExpired = false;
            vm.IsVendorRecommendationChanges = false;
            vm.isVendorNotLinkedToAnyProperty = false;
            vm.canReceiveMonthlyReport = false;
logc("notificationsData");
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            pubsub.subscribe("diq.canReceiveMonthlyReport", vm.updateReportVlaue);
        };

        vm.isActive = function () {
            return productDataModel.isPropertyGridActive() || productDataModel.isRoleGridActive();
        };

        vm.isVendorCredentialing = function () {
            if ($scope.$parent.productId === 16) {
                return true;
            }
            return false;
        };

        vm.isResidentPortal = function () {
            if ($scope.$parent.productId === 17) {
                return true;
            }
            return false;
        };

        vm.isDepositIQ = function () {
            if ($scope.$parent.productId === 47) {
                return true;
            }
            return false;
        };

        vm.hasViewOnlyAccess = function () {
            return security.isAllowed("viewUser") || syncMgr.isUserHasManageProductAccess($scope.$parent.productId);
        };

        vm.loadData = function () {
            var productId = $scope.$parent.productId;
            logc("vm.isActive()",vm.isActive());
            if (persona.isReady() && vm.isActive()) {
                var notificationsData = syncMgr.getProductNotificationsData(productId);
                if (notificationsData === undefined) {
                    var params = {
                        userPersonaId: userDetailsModel.getPersonaId(),
                        editorPersonaId: persona.getId()
                    };
                    if (productId === 16) {
                        vm.datareq = notificationSvc.get(params, vm.setNotificationsData);
                    }
                    if (productId === 17) {
                        vm.datareq = dataSvc.get(params, vm.setNotificationsData);
                    }
                }

                if (productId === 47) {
                    vm.setSwitchStatus();
                }
            }
        };

        vm.setNotificationsData = function (notificationData) {
            var productId = $scope.$parent.productId;
            vm.setSwitchStatus();
            syncMgr.setProductAllNotifications(productId, notificationData.data);
            if (productId === 17) {
                vm.frontDesk = notificationData.data.managerFdiViaEmail;
                vm.amenity = notificationData.data.amenitiesViaEmail;
                vm.serviceReq = notificationData.data.managerMrViaEmail;
            }
            if (productId === 16) {
                syncMgr.setProductAllNotifications(productId, notificationData);
                vm.isInsuranceExpired = notificationData.isInsuranceExpired;
                vm.IsVendorRecommendationChanges = notificationData.isVendorRecommendationChanges;
                vm.isVendorNotLinkedToAnyProperty = notificationData.isVendorNotLinkedToAnyProperty;
            }
        };

        vm.setSwitchStatus = function () {
            var productId = $scope.$parent.productId;
            if (productId === 17) {
                vm.frontDeskSwitch = switchConfig({
                    disabled: vm.hasViewOnlyAccess(),
                    onChange: vm.setFrontDeskSwitch
                });

                vm.amenitySwitch = switchConfig({
                    disabled: vm.hasViewOnlyAccess(),
                    onChange: vm.setAmenitySwitch
                });

                vm.serviceReqSwitch = switchConfig({
                    disabled: vm.hasViewOnlyAccess(),
                    onChange: vm.setServiceReqSwitch
                });
            }
            if (productId === 16) {
                vm.venIsInsuranceExpired = switchConfig({
                    disabled: vm.hasViewOnlyAccess(),
                    onChange: vm.setisInsuranceExpired
                });
                vm.venIsVendorRecommendationChanges = switchConfig({
                    disabled: vm.hasViewOnlyAccess(),
                    onChange: vm.setisVendorRecommendationChanges
                });
                vm.venIsVendorNotLinkedToAnyProperty = switchConfig({
                    disabled: vm.hasViewOnlyAccess(),
                    onChange: vm.setisVendorNotLinkedToAnyProperty
                });
            }
            if (productId === 47) {
                vm.dIQswitchconfigs = syncMgr.getProductSwitchConfig(productId, "Notifications");

                if (vm.dIQswitchconfigs !== undefined && vm.dIQswitchconfigs.length > 0) {
                    vm.dIQswitchconfigs.forEach(function (item) {
                        item.configData = switchConfig({
                            disabled: vm.hasViewOnlyAccess(),
                            onChange: vm.setCanReceiveMonthlyReport
                        });
                    });
                }
                vm.canReceiveMonthlyReport = syncMgr.getCanReceiveMonthlyReport();
                logc("vm.dIQswitchconfigs",vm.dIQswitchconfigs, vm.canReceiveMonthlyReport);
            }
        };

        vm.setCanReceiveMonthlyReport = function (val) {
            vm.canReceiveMonthlyReport = val;
            syncMgr.setCanReceiveMonthlyReport(val);
        };

        vm.setFrontDeskSwitch = function (val) {
            vm.frontDesk = val;

            var productId = $scope.$parent.productId;
            var notificationsData = syncMgr.getProductNotificationsData(productId);
            notificationsData.managerFdiViaEmail = val;
            syncMgr.setProductAllNotifications(productId, notificationsData);
        };

        vm.setAmenitySwitch = function (val) {
            vm.amenity = val;

            var productId = $scope.$parent.productId;
            var notificationsData = syncMgr.getProductNotificationsData(productId);
            notificationsData.amenitiesViaEmail = val;
            syncMgr.setProductAllNotifications(productId, notificationsData);
        };

        vm.setServiceReqSwitch = function (val) {
            vm.serviceReq = val;

            var productId = $scope.$parent.productId;
            var notificationsData = syncMgr.getProductNotificationsData(productId);
            notificationsData.managerMrViaEmail = val;
            syncMgr.setProductAllNotifications(productId, notificationsData);
        };

        vm.setisInsuranceExpired = function (val) {
            var productId = $scope.$parent.productId;
            var notificationsData = syncMgr.getProductNotificationsData(productId);
            notificationsData.isInsuranceExpired = val;
            syncMgr.setProductAllNotifications(productId, notificationsData);
        };
        vm.setisVendorRecommendationChanges = function (val) {
            var productId = $scope.$parent.productId;
            var notificationsData = syncMgr.getProductNotificationsData(productId);
            notificationsData.isVendorRecommendationChanges = val;
            syncMgr.setProductAllNotifications(productId, notificationsData);
        };
        vm.setisVendorNotLinkedToAnyProperty = function (val) {
            var productId = $scope.$parent.productId;
            var notificationsData = syncMgr.getProductNotificationsData(productId);
            notificationsData.isVendorNotLinkedToAnyProperty = val;
            syncMgr.setProductAllNotifications(productId, notificationsData);
        };

        vm.updateReportVlaue = function (val) {
            vm.canReceiveMonthlyReport = val;
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
            "vendCompNotificationsSvc",
            "routeSecurity",
            "personaDetails",
            "productDataSyncManager",
            "productPanelDataModel",
            "userDetailsModel",
            "rpSwitchConfig",
            "pubsub",
            ProductNotificationGridCtrl
        ]);
})(angular);

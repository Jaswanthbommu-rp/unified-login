(function (angular, undefined) {
    "use strict";

    function ProductNotificationGridCtrl($scope, $filter, vendCompDataModel, dataSvc, notificationSvc, security, persona, syncMgr, productDataModel, userDetailsModel, switchConfig) {
        var vm = this;

        vm.init = function () {
            vm.frontDesk = false;
            vm.amenity = false;
            vm.serviceReq = false;
            vm.isInsuranceExpired = false;
            vm.isVendorRecommendationChanges = false;
            vm.isVendorNotLinkedToAnyProperty = false;
            vm.vendCompDataModel = vendCompDataModel;

            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.isActive = function () {
            return productDataModel.isPropertyGridActive();
        };

        vm.isVendorCredentialing = function () {
            if($scope.$parent.productId === 16){
                return true;
            }
            return false;
        };

        vm.hasViewOnlyAccess = function () {
            return security.isAllowed("viewUser") || syncMgr.isUserHasManageProductAccess($scope.$parent.productId);
        };


        vm.loadData = function () {
            var productId = $scope.$parent.productId;
            if (persona.isReady() && vm.isActive()) {
                var notificationsData = syncMgr.getProductNotificationsData(productId);
                if (notificationsData === undefined) {
                    var params = {
                            userPersonaId: userDetailsModel.getPersonaId(),
                            editorPersonaId: persona.getId()
                        };
                        if(productId === 16) {
                            vm.datareq = notificationSvc.get(params, vm.setNotificationsData);
                        }
                        if(productId === 17) {
                            vm.datareq = dataSvc.get(params, vm.setNotificationsData);
                        }
                        
                }
            }
        };

        vm.setNotificationsData = function(notificationData){
            var productId = $scope.$parent.productId;
            vm.setSwitchStatus();
            if(productId === 17) {
                syncMgr.setProductAllNotifications(productId, notificationData.data);
                vm.frontDesk = notificationData.data.managerFdiViaEmail;
                vm.amenity = notificationData.data.amenitiesViaEmail;
                vm.serviceReq = notificationData.data.managerMrViaEmail;
            }
            if(productId === 16) {
                syncMgr.setProductAllNotifications(productId, notificationData);
            }
            
        };

        vm.setSwitchStatus = function() {
            var productId = $scope.$parent.productId;
            if(productId === 17) {
            vm.frontDeskSwitch = switchConfig({
                disabled: security.isAllowed("viewUser") || !persona.data.hasResidentPortalUserAccess,
                onChange: vm.setFrontDeskSwitch
            });

            vm.amenitySwitch = switchConfig({
                disabled: security.isAllowed("viewUser") || !persona.data.hasResidentPortalUserAccess,
                onChange: vm.setAmenitySwitch
            });

            vm.serviceReqSwitch = switchConfig({
                disabled: security.isAllowed("viewUser") || !persona.data.hasResidentPortalUserAccess,
                onChange: vm.setServiceReqSwitch
            });
         }
         if(productId === 16) {
            vm.venIsInsuranceExpired = switchConfig({
                disabled: security.isAllowed("viewUser") || syncMgr.isUserHasManageProductAccess($scope.$parent.productId),
                onChange: vm.setisInsuranceExpired
            });

            vm.venIsVendorRecommendationChanges = switchConfig({
                disabled: security.isAllowed("viewUser") || syncMgr.isUserHasManageProductAccess($scope.$parent.productId),
                onChange: vm.setisVendorRecommendationChanges
            });

            vm.venIsVendorNotLinkedToAnyProperty = switchConfig({
                disabled: security.isAllowed("viewUser") || syncMgr.isUserHasManageProductAccess($scope.$parent.productId),
                onChange: vm.setisVendorNotLinkedToAnyProperty
            });
         }
        };

        vm.setFrontDeskSwitch = function(val){
            vm.frontDesk = val;

            var productId = $scope.$parent.productId;
            var notificationsData = syncMgr.getProductNotificationsData(productId);
            notificationsData.managerFdiViaEmail = val;
            syncMgr.setProductAllNotifications(productId, notificationsData);
        };

        vm.setAmenitySwitch = function(val){
            vm.amenity = val;

            var productId = $scope.$parent.productId;
            var notificationsData = syncMgr.getProductNotificationsData(productId);
            notificationsData.amenitiesViaEmail = val;
            syncMgr.setProductAllNotifications(productId, notificationsData);
        };

        vm.setServiceReqSwitch = function(val){
            vm.serviceReq = val;

            var productId = $scope.$parent.productId;
            var notificationsData = syncMgr.getProductNotificationsData(productId);
            notificationsData.managerMrViaEmail = val;
            syncMgr.setProductAllNotifications(productId, notificationsData);
        };

        vm.setisInsuranceExpired = function(val){
            var productId = $scope.$parent.productId;
            var notificationsData = syncMgr.getProductNotificationsData(productId);
            notificationsData.isInsuranceExpired = val;
            syncMgr.setProductAllNotifications(productId, notificationsData);
        };

        vm.setisVendorRecommendationChanges = function(val){
            var productId = $scope.$parent.productId;
            var notificationsData = syncMgr.getProductNotificationsData(productId);
            notificationsData.isVendorRecommendationChanges = val;
            syncMgr.setProductAllNotifications(productId, notificationsData);
        };

        vm.setisVendorNotLinkedToAnyProperty = function(val){
            var productId = $scope.$parent.productId;
            var notificationsData = syncMgr.getProductNotificationsData(productId);
            notificationsData.isVendorNotLinkedToAnyProperty = val;
            syncMgr.setProductAllNotifications(productId, notificationsData);
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
            "vendorComplianceDataModel",
            "resPortNotificationsSvc",
            "vendCompNotificationsSvc",
            "routeSecurity",
            "personaDetails",
            "productDataSyncManager",
            "productPanelDataModel",
            "userDetailsModel",
            "rpSwitchConfig",
            ProductNotificationGridCtrl
        ]);
})(angular);
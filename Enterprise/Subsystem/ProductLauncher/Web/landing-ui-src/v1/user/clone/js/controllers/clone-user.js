//  Clone User Controller

(function (angular, undefined) {
    "use strict";

    function CloneUserCtrl($scope, $params, $location, timeout, tabsModel, tabsManager, cloneUserSvc, userView, userDetails, userStatus, statusMsg, statusMsgModel, formConfig, security, session, dashboard, thirdPartyIDP, userCustomField, customFieldsModel, pubsub, productSvc) {
        var vm = this;

        vm.init = function () {
            vm.security = security;
            userView.setCloneMode();
            userDetails.setClonedUser(true);
            vm.editingSelf = false;
            vm.customFieldList = [];

            vm.setTabs().setCallbacks();
            vm.setFromDate();
           // vm.setDefaulttimezone();

            if (session.isReady()) {
                vm.onSessionReady();
                vm.sessionWatch = angular.noop;
            }
            else {
                vm.sessionWatch = session.subscribe(vm.onSessionReady);
            }

            vm.get3rdPartyIDPSetting();
            vm.getUserCustomFields();

            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        // Getters

        vm.get3rdPartyIDPSetting = function () {
            thirdPartyIDP.get(vm.init3rdPartyIDPSetting);
        };

        vm.getUserCustomFields = function () {
            var reqData = {};

            vm.userCustomFieldReq = userCustomField.get(reqData, vm.initUserCustomFields);
            return vm;
        };

        // Setters

        vm.setCallbacks = function () {
            tabsManager.setSaveErrorCallback(vm.showStatusMsg);
            tabsManager.setSaveSuccessCallback(vm.onSaveSuccess);
            return vm;
        };

        vm.setDateMinLimit = function () {
            formConfig.setThruDateMinLimit(userDetails.getThruDateMinLimit());
            formConfig.setFromDateMinLimit(userDetails.getFromDateMinLimit());
        };

        vm.setFromDate = function () {
            userDetails.setFromDate();
        };

        // vm.setDefaulttimezone = function () {
        //     userDetails.setDefaulttimezone();
        // };

        vm.setTabs = function () {
            var tabs = ["userDetails"];

            if (userStatus.isRegularUser() && !userDetails.isDisabled()) {
                tabs.push("productAccess");
            }

            tabsModel.setTabs(tabs).activateTab("userDetails");

            return vm;
        };

        // Actions

        vm.init3rdPartyIDPSetting = function (resp) {
            userDetails
                .set3rdPartyIDPVisible(resp.data.isLocal);
        };

        vm.initUserCustomFields = function (resp) {
            var cfData = resp.records || [];
            cfData.forEach(function (data) {
                    var customfield = customFieldsModel(data);
                    vm.customFieldList.push(customfield);
                });

            userDetails.setUserCustomFieldData(vm.customFieldList);
        };

        vm.initUser = function () {
            vm.getUserReq = cloneUserSvc.get($params, vm.onGetUser);
        };

        vm.checkAccess = function () {
            var activity,
                allowed = true;

            if (session.getRealPageId() === $params.realPageId) {
                activity = "editSelf";
                vm.editingSelf = true;
            }
            else {
                activity = "editOther";
            }

            allowed = security.isAllowed("editSelf") ||
                security.isAllowed("editOther") ||
                security.isAllowed("editUser") ? true : false;

            if (!allowed) {
                $location.path("/error/access-denied");
            }

            return allowed;
        };

        vm.onGetUser = function (resp) {
            var data = resp.data;
            //data.realPageId = $params.realPageId;

            userDetails.setData(data);

            userStatus.setStatusId(data.userTypeId);
            pubsub.publish("settings.userDataReady");

            vm.setTabs();
            tabsManager.init();

            timeout(vm.setDateMinLimit, 100);
            if (data.userTypeId === 404){
                    vm.setProductsData($params.realPageId);
            }
        };

        vm.onSessionReady = function () {
            if (vm.checkAccess()) {
                vm.initUser();
            }
        };

        vm.onSaveSuccess = function (data) {
            if (vm.editingSelf) {
                vm.sessionWatch();
                session.reload();
                dashboard.reload();
            }

            vm.showStatusMsg(data);
        };

        vm.onGetProductData = function (resp) {
            var message = "";
            var products = [];
            if (resp.data) {
                resp.data.forEach(function (family) {
                    if (family.familyId !== 500) {
                        family.solutions.forEach(function (soln) {
                            if (soln.isAssigned && soln.notificationEmailRequiredForUserWithNoEmail) {
                                products.push(soln.titleId);
                            }
                        });
                    }
                });
            }

            if (products.length > 0) {
                message = products.join();
                formConfig.setNotificationEmailRequired(true, message);
            }
            pubsub.publish("user.assignedproducts", resp);
        };

        vm.showStatusMsg = function (data) {
            statusMsgModel.setData({
                tabStatus: data,
                userExists: false
            });

            statusMsg.show();
        };

        vm.setProductsData = function (realpageID) {
            var reqData = {
                   personRealPageId: realpageID,
                   accessFilter: "UserDetails"
                };

            productSvc.get(reqData, vm.onGetProductData);
        };


        // Destroy

        vm.destroy = function () {
            vm.destWatch();
            userView.reset();
            userDetails.reset();
            tabsModel.reset();
            vm.sessionWatch();
            vm.getUserReq.$cancelRequest();
            vm.userCustomFieldReq.$cancelRequest();
            tabsManager.setSaveErrorCallback(angular.noop);
            tabsManager.setSaveSuccessCallback(angular.noop);

            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("CloneUserCtrl", [
            "$scope",
            "$stateParams",
            "$location",
            "timeout",
            "userTabsModel",
            "userTabsManager",
            "cloneUserSvc",
            "userViewModel",
            "userDetailsModel",
            "userStatusModel",
            "userReqStatusMsgModal",
            "userReqStatusMsgModel",
            "userDetailsFormConfig",
            "routeSecurity",
            "userSessionModel",
            "dashboardModel",
            "thirdPartyIDPSvc",
            "userCustomFieldSvc",
            "userCustomFieldsModel",
            "pubsub",
            "assignProductsSvc",
            CloneUserCtrl
        ]);
})(angular);

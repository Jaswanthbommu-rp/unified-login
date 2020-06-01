//  Edit User Controller

(function (angular, undefined) {
    "use strict";

    function EditUserCtrl($scope, $params, $location, timeout, tabsModel, tabsManager, userSvc, userView, userDetails, userStatus, statusMsg, statusMsgModel, formConfig, security, session, dashboard, thirdPartyIDP, moment, userCustomField, customFieldsModel, pubsub, productSvc) {
        var vm = this;

        vm.init = function () {
            vm.security = security;
            userView.setEditMode();
            vm.editingSelf = false;
            vm.customFieldList = [];

            vm.setTabs().setCallbacks();

            if (session.isReady()) {
                vm.onSessionReady();
                vm.sessionWatch = angular.noop;
            }
            else {
                vm.sessionWatch = session.subscribe(vm.onSessionReady);
            }

            vm.get3rdPartyIDPSetting();

            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        // Getters

        vm.get3rdPartyIDPSetting = function () {
            thirdPartyIDP.get(vm.init3rdPartyIDPSetting);
        };

        // Setters

        vm.setCallbacks = function () {
            tabsManager.setSaveErrorCallback(vm.showStatusMsg);
            tabsManager.setSaveSuccessCallback(vm.onSaveSuccess);
            return vm;
        };

        vm.setDateMinLimit = function () {
            formConfig.setThruDateMinLimit(userDetails.getThruDateMinLimit());
            if (userDetails.isDisabled()) {
                var limit = moment().startOf('day');
                formConfig.setFromDateMinLimit(limit);
            }
            else {
                formConfig.setFromDateMinLimit(userDetails.getFromDateMinLimit());
            }

        };

        vm.setTabs = function () {
            var tabs = ["userDetails"];

            if (userStatus.isRegularUser() && !userDetails.isDisabled()) {
                tabs.push("productAccess");
            }

            if (!userDetails.isDisabled() &&
                !userDetails.is3rdPartyIDP()) {
                tabs.push("securityQuestions");
            }

            if (!userDetails.is3rdPartyIDP()) {
                tabs.push("resetPassword");
            }

            if (security.isAllowed("viewAuditTrailUserData")) {
                tabs.push("activityLog");
            }

            tabsModel.setTabs(tabs).activateTab("userDetails");

            return vm;
        };

        // Actions

        vm.init3rdPartyIDPSetting = function (resp) {
            userDetails
                .set3rdPartyIDPVisible(resp.data.isLocal);
        };

        vm.initUser = function () {
            vm.getUserReq = userSvc.get($params, vm.onGetUser);
        };

        vm.checkAccess = function () {
            var activity,
                allowed = true;

            if (session.getRealPageId() === $params.realPageId) {
                vm.editingSelf = true;
            }

            return allowed;
        };

        vm.onGetUser = function (resp) {
            var data = resp.data;

            if (resp.status.success) {
                data.realPageId = $params.realPageId;
                timeout(vm.setUserCustomFields(data.customFields), 100);
                userDetails.setData(data);
                userStatus.setStatusId(data.userTypeId);
                pubsub.publish("settings.userDataReady");

                vm.setTabs();

                tabsManager.init();
                timeout(vm.setDateMinLimit, 100);
                vm.setProductsData(data.realPageId);
                // if (data.userTypeId === 404){
                //     vm.setProductsData(data.realPageId);
                // }
            }
            else {
                vm.invalidateEdit(resp.status);
            }
        };

        vm.setProductsData = function (realpageID) {
            var reqData = {
                   personRealPageId: realpageID,
                   accessFilter: "UserDetails"
                };

            productSvc.get(reqData, vm.onGetProductData);
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

            if (products.length > 0 && userStatus.isRegularUserNoEmail()) {
                message = products.join();
                formConfig.setNotificationEmailRequired(true, message);
            }
            pubsub.publish("user.assignedproducts", resp);
        };

        vm.invalidateEdit = function (status) {
            var modalText = [];
            modalText.push({
                success: false,
                errorCode: status.errorCode
            });

            vm.showStatusMsg(modalText);
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

        vm.setUserCustomFields = function (data) {
            var cfData = data || [];
            cfData.forEach(function (data) {
                var customfield = customFieldsModel(data);
                vm.customFieldList.push(customfield);
            });
            userDetails.setUserCustomFieldData(vm.customFieldList);
        };

        vm.showStatusMsg = function (data) {
            statusMsgModel.setData({
                tabStatus: data,
                userExists: true,
                errorCode: data[0].errorCode
            });

            statusMsg.show();
        };

        // Destroy

        vm.destroy = function () {
            vm.destWatch();
            userView.reset();
            tabsModel.reset();
            userDetails.reset();
            vm.sessionWatch();
            if (vm.getUserReq) {
                vm.getUserReq.$cancelRequest();
            }
            tabsManager.setSaveErrorCallback(angular.noop);
            tabsManager.setSaveSuccessCallback(angular.noop);

            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("EditUserCtrl", [
            "$scope",
            "$stateParams",
            "$location",
            "timeout",
            "userTabsModel",
            "userTabsManager",
            "userSvc",
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
            "moment",
            "userCustomFieldSvc",
            "userCustomFieldsModel",
            "pubsub",
            "assignProductsSvc",
            EditUserCtrl
        ]);
})(angular);

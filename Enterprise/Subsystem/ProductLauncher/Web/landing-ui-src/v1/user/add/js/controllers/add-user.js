//  Add User Controller

(function (angular, undefined) {
    "use strict";

    function AddUserCtrl($scope, timeout, moment, tabsModel, tabsManager, userStatus, userDetails, statusMsg, statusMsgModel, formConfig, thirdPartyIDP, userCustomField, customFieldsModel) {
        var vm = this;

        vm.init = function () {
            vm.customFieldList = [];
            vm.setCallbacks();
            timeout(vm.setDateMinLimit, 10);
            vm.setFromDate();
            //vm.setDefaulttimezone();

            tabsModel.setTabs([
                "userDetails",
                "productAccess"
            ]);

            tabsManager.init();
            tabsModel.activateTab("userDetails");

            vm.get3rdPartyIDPSetting();
            vm.getUserCustomFields();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        // Getters

        vm.get3rdPartyIDPSetting = function () {
            thirdPartyIDP.get(vm.initThirdPartyIDPSetting);
        };

        vm.getUserCustomFields = function () {
            var reqData = {};

            vm.userCustomFieldReq = userCustomField.get(reqData, vm.initUserCustomFields);
            return vm;
        };

        // Setters

        vm.setDateMinLimit = function () {
            formConfig.setThruDateMinLimit(userDetails.getThruDateMinLimit());
            formConfig.setFromDateMinLimit(userDetails.getFromDateMinLimit());
        };

        vm.setCallbacks = function () {
            tabsManager.setSaveErrorCallback(vm.showStatusMsg);
            tabsManager.setSaveSuccessCallback(vm.showStatusMsg);
        };

        vm.setFromDate = function () {
            userDetails.setFromDate();
        };

        // vm.setDefaulttimezone = function () {
        //     userDetails.setDefaulttimezone();
        // };
        // Actions
         vm.initUserCustomFields = function (resp) {
            var cfData = resp.records || [];
            cfData.forEach(function (data) {
                    var customfield = customFieldsModel(data);
                    vm.customFieldList.push(customfield);
                });

            userDetails.setUserCustomFieldData(vm.customFieldList);
        };

        vm.initThirdPartyIDPSetting = function (resp) {
            userDetails
                .set3rdPartyIDP(!resp.data.isLocal)
                .set3rdPartyIDPVisible(resp.data.isLocal);
        };

        vm.showStatusMsg = function (data) {
            statusMsgModel.setData({
                tabStatus: data,
                userExists: false
            });

            statusMsg.show();
        };

        // Destroy

        vm.destroy = function () {
            vm.destWatch();
            tabsModel.reset();
            tabsManager.setSaveErrorCallback(angular.noop);
            tabsManager.setSaveSuccessCallback(angular.noop);

            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("AddUserCtrl", [
            "$scope",
            "timeout",
            "moment",
            "userTabsModel",
            "userTabsManager",
            "userStatusModel",
            "userDetailsModel",
            "userReqStatusMsgModal",
            "userReqStatusMsgModel",
            "userDetailsFormConfig",
            "thirdPartyIDPSvc",
            "userCustomFieldSvc",
            "userCustomFieldsModel",
            AddUserCtrl
        ]);
})(angular);

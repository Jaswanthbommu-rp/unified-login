(function(angular) {
	"use strict";

	var changePwdController = function($scope, $q, $filter, passwordModel, userPasswordState, changePasswordSvc, 
            layoutModel, loginModel, userModel, passwordConfig, passwordData,PasswordPolicySvc,userDetSvc) {
        var vm = this;

        vm.init = function () {
            vm.errorMessages = [];
            vm.formState = {
                password: null,
                isSaving: false
            };

            vm.label = {
                changePasswordForUser: $filter("changePasswordText")("label.instructions")
            };

            var username = userModel.getEnterpriseLoginName();
            passwordModel.setData(passwordData);
            vm.changePasswordForm = null;

            vm.model = passwordData;
            vm.updateUser(username);

            vm.getUserInfo(username);
            vm.passcheck = false;
            vm.limitedHistoryDeferred = null;
            vm.passwordData = passwordData;
            vm.passwordConfig = passwordConfig;
            passwordConfig.setMethodsSrc(vm);
            vm.destroyCtrl = $scope.$on("$destroy", vm.destroy);
        };

        vm.updateUser = function(username) {
            vm.label.changePasswordForUser = vm.label.changePasswordForUser.replace("{{email}}", username);
        };

        vm.displayErrorMsg = function() {
            return vm.changePasswordForm.$submitted; 
        };

        vm.getUserInfo = function(enterPriseUserName){
            var params = {
                enterpriseUserName: enterPriseUserName
            };
            userDetSvc.get(params, vm.onDataReady, vm.setDataErr);

        };
        
        vm.onDataReady = function (resp) {
            if (resp.isError === false) {
                if (resp.records.length > 0) {
                    vm.validatePasswordPolicy(resp.records[0].organizationPartyId);
                }
            }
        };

        vm.setDataErr = function (resp) {
            alert(resp);
        };

        vm.validatePasswordPolicy = function(orgPartyId) {
            var params = {
                    PartyId: orgPartyId
            };
            PasswordPolicySvc.get(params, vm.onResponseReady, vm.setDataErr);
        };
        vm.onResponseReady = function (resp) {
                var settings = resp.data;
                vm.formState.password = userPasswordState.init(settings);
        };

        vm.changePassword = function() {
            vm.changePasswordForm.$setSubmitted();
            vm.changePasswordForm.confirmPassword.$validate();
            if(vm.changePasswordForm.$valid) {
                vm.toggleSaving();
                changePasswordSvc.savePassword(vm.model)
                    .then(vm.checkResponse)
                    .finally(vm.toggleSaving);
            }

            return;
        };

        vm.toggleSaving = function(flag) {
            if(flag === undefined) {
                vm.formState.isSaving = !vm.formState.isSaving;
            } else {
                vm.formState.isSaving = flag;
            }
        };

        vm.checkResponse = function (data) {
            if (!data.isError && data.isSuccess === true) {
                vm.redirectToLogin();
            } else {
                vm.errorMessages.push(data.errorReason);
            }
        };

        vm.redirectToLogin = function() {
            loginModel.displayPasswordChangedMessage();
            layoutModel.setActiveState("login");
        };

        vm.passChecklist = function(modelValue) {
            var ocheckList =  userPasswordState.isPasswordValid(modelValue);
            if (ocheckList === true) {
                vm.passcheck = true;
            } else {
                vm.passcheck = false;
            }
            return ocheckList;
        };

        vm.confirmNewPassword = function(modelValue) {
            var confirmPwd = passwordData.confirmPassword;
            if ((confirmPwd === "" || modelValue === "") || (confirmPwd !== modelValue && vm.passcheck === false) || (confirmPwd === modelValue && vm.passcheck === true) || (confirmPwd !== modelValue && vm.passcheck === true)) {
                return true;
            }
        };

        vm.noUsername = function(modelValue) {
            var username = userModel.getEnterpriseLoginName().toLowerCase(),
                smallModel = modelValue.toLowerCase();
            if (vm.passcheck !== true && username === smallModel) {
                return true;
            }
            return username != smallModel;
        };

        vm.isEmpty = function (data) {
            return !(angular.isString(data) && data.length > 0);
        };

        vm.limitedHistory = function (modelValue) {
            if (vm.isEmpty(modelValue)) {
                return $q.resolve();
            }

            if (vm.limitedHistoryDeferred) {
                vm.limitedHistoryDeferred.resolve();
            }
            vm.limitedHistoryDeferred = $q.defer();

            changePasswordSvc.isUnique(modelValue)
                .then(vm.limitedHistoryValidation);
            return vm.limitedHistoryDeferred.promise;
        };

        vm.limitedHistoryValidation = function (response) {
            if (response.isSuccess) {
                vm.limitedHistoryDeferred.resolve();
            } else {
                vm.limitedHistoryDeferred.reject();
            }
        };

        vm.confirmValidPassword = function(modelValue) {
            var newPwd = passwordData.createPassword,
                returnValue = newPwd === modelValue;
            if (modelValue === "") {
                return true;
            }
            if (newPwd !== modelValue) {
                return returnValue;
            } else {
                return returnValue;
            }
        };

        vm.resetErrorMsgs = function() {
            vm.errorMessages = [];
        };

        vm.destroy = function () {
            vm.resetErrorMsgs();

            vm.changePasswordForm = undefined;

            vm.model = undefined;

            vm.destroyCtrl();
            vm.destroyCtrl = undefined;

            userPasswordState.reset();
            userModel.reset();
            passwordModel.reset();
            vm.passwordData = undefined;
            vm.passwordConfig = undefined;
            vm.passcheck = undefined;
            vm.limitedHistoryDeferred = undefined;
            vm = undefined;
        };

        vm.init();
    };

    angular
        .module("identity")
        .controller("ChangePwdController", [
            "$scope",
            "$q",
            "$filter",
            "changePasswordModel",
            "userPasswordState",
            "changePasswordSvc",
            "layoutModel",
            "loginModel",
            "userModel",
            "userPasswordConfig",
            "changePasswordData",
            "PasswordPolicySvc",
            "getUserSvc",
            changePwdController
        ]);
	
})(angular);
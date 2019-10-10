(function (angular) {
    "use strict";

    var changePwdController = function (
        $scope,
        $q,
        $filter,
        $window,
        $location,
        userModel,
        passwordData,
        passwordModel,
        userPasswordState,
        passwordSvc,
        rpWatchList,
        passwordConfig,
        passwordDetails,
        headerActions,
        userAcctNfnSvc) {
        var vm = this;

        vm.init = function () {
            vm.errorMessages = [];
            vm.formState = {
                password: null,
                isSaving: false,
                canCancel: true,
                title: $filter("changePasswordText")("label.instructions")
            };

            vm.ready = false;
            vm.updateUser();
            vm.passwordConfig = passwordConfig;
            passwordConfig.setMethodsSrc(vm);
            passwordModel.setData(passwordData);
            vm.changePasswordForm = null;
            vm.model = passwordData;
            vm.formState.password = userPasswordState.init();
            vm.formState.canCancel = true; //TODO (userModel.getAccountExpiry() > 0);
            vm.passwordData = passwordData;
            vm.passcheck = false;
            vm.limitedHistoryDeferred = null;
            vm.existingPasswordDeferred = null;
            vm.incorrectOldPassword = false;
            vm.watchList = rpWatchList();
            vm.watchList.add($scope.$on("$destroy", vm.destroy));
            var clearpw = setTimeout(vm.clearNewPasswordFields, 400);
        };

        vm.clearNewPasswordFields = function() {
            vm.model.createPassword = "";
            vm.model.confirmPassword = "";
            vm.passwordData.createPassword = "";
            vm.passwordData.confirmPassword = "";
            vm.ready = true;
        };

        vm.updateUser = function () {
            if (userModel.isReady()) {
                vm.formState.title = vm.formState.title.replace(":username", userModel.getUsername());

                 if (userModel.isPasswordExpired()) {
                    userAcctNfnSvc.setPwdExpired(true);
                }
                userAcctNfnSvc.notifyPwdExpired();

                if (vm.sessionWatch) {
                    vm.sessionWatch();
                    vm.sessionWatch = undefined;
                }
            }
            else {
                vm.sessionwatch = userModel.subscribe(vm.updateUser);
            }
        };

        vm.changePassword = function () {
            vm.changePasswordForm.$setSubmitted();
            if (vm.changePasswordForm.$valid) {
                vm.toggleSaving();
                passwordSvc.savePassword(vm.model)
                    .then(vm.checkResponse)
                    .finally(vm.toggleSaving);
            }
            return;
        };

        vm.toggleSaving = function (flag) {
            if (flag === undefined) {
                vm.formState.isSaving = !vm.formState.isSaving;
            }
            else {
                vm.formState.isSaving = flag;
            }
        };

        vm.checkResponse = function (data) {
            if (!data.isError && data.isSuccess === true) {
                headerActions.signout({msgId: 201});
            }
            else {
                if (data.errorReason === "Current password is incorrect.") {
                    vm.incorrectOldPassword = true;
                }
                vm.errorMessages.push(data.errorReason);
            }
        };

        vm.isEmpty = function (data) {
            return !(angular.isString(data) && data.length > 0);
        };

        vm.passChecklist = function (modelValue) {
            var ocheckList = userPasswordState.isPasswordValid(modelValue);
            if (ocheckList === true) {
                vm.passcheck = true;
            }
            else {
                vm.passcheck = false;
            }
            return ocheckList;
        };

        vm.confirmNewPassword = function (modelValue) {
            var confirmPwd = passwordData.confirmPassword;
            if ((confirmPwd === "" || modelValue === "") || (confirmPwd !== modelValue && vm.passcheck === false) || (confirmPwd === modelValue && vm.passcheck === true) || (confirmPwd !== modelValue && vm.passcheck === true)) {
                userPasswordState.updateUniqueness(false);
                return true;
            }
        };

        vm.confirmValidPassword = function (modelValue) {
            var newPwd = passwordData.createPassword,
                returnValue = newPwd === modelValue;
            if (modelValue === "") {
                return true;
            }
            if (newPwd !== modelValue) {
                return returnValue;
            }
            else {
                return returnValue;
            }
        };

        vm.noUsername = function (modelValue) {
            if (!userModel.isReady()) {
                return false;
            }

            var username = userModel.getUsername().toLowerCase(),
                smallModel = modelValue.toLowerCase();
            if (vm.passcheck !== true && username === smallModel) {
                return true;
            }
            return username != smallModel;
        };

        vm.limitedHistory = function (modelValue) {
            if (vm.isEmpty(modelValue) || !vm.ready) {
                return $q.resolve(); //consider empty model valid
            }

            if (vm.limitedHistoryDeferred) {
                vm.limitedHistoryDeferred.resolve();
            }
            vm.limitedHistoryDeferred = $q.defer();
            passwordSvc.isUnique(modelValue)
                .then(vm.limitedHistoryValidation);
            return vm.limitedHistoryDeferred.promise;
        };

        vm.limitedHistoryValidation = function (response) {
            if (response.isSuccess) {
                userPasswordState.updateUniqueness(true);
                vm.limitedHistoryDeferred.resolve();
            }
            else {
                userPasswordState.updateUniqueness(false);
                vm.limitedHistoryDeferred.reject();
            }
        };

        vm.goBack = function () {
            if (userModel.resetPasswordRequired() || userModel.isPasswordExpired()) {
                headerActions.signout();
            }
            else {
                $window.history.back();
            }
        };

        vm.resetErrorMsgs = function () {
            vm.errorMessages = [];
        };

        vm.destroy = function () {
            vm.resetErrorMsgs();

            vm.watchList.destroy();
            vm.watchList = undefined;
            passwordModel.reset();
            vm.formState.password = undefined;
            vm.formState = undefined;
            vm.passcheck = undefined;
            vm.limitedHistoryDeferred = undefined;
            vm.existingPasswordDeferred = undefined;
            vm.model = undefined;
            vm.changePasswordForm = undefined;
            vm.passwordData = undefined;
            vm.passwordConfig = undefined;
            vm.incorrectOldPassword = undefined;
            vm = undefined;
        };

        vm.init();
    };

    angular
        .module("settings")
        .controller("ChangePwdController", [
            "$scope",
            "$q",
            "$filter",
            "$window",
            "$location",
            "userSessionModel",
            "changePasswordData",
            "changePasswordModel",
            "userPasswordState",
            "changePasswordSvc",
            "rpWatchList",
            "userPasswordConfig",
            "passwordDetails",
            "rpGlobalHeaderActions",
            "userAccountNotificationSvc",
            changePwdController
        ]);

})(angular);

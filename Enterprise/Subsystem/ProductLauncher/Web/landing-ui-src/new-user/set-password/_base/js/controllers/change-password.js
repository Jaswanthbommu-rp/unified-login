(function (angular) {
    "use strict";

    var ChangePwdController = function ($scope, $q, $state, $stateParams, $window, cookie, passwordModel, userPasswordState, changePasswordSvc, userModel, passwordConfig, passwordData) {
        var vm = this;

        vm.init = function () {
            vm.destroyCtrl = $scope.$on("$destroy", vm.destroy);

            vm.errorMessages = "";
            vm.formState = {
                password: userPasswordState.init(),
                isSaving: false
            };

            cookie.erase('access_token');
            passwordModel.setData(passwordData);
            vm.changePasswordForm = null;

            vm.validatePasswordDeferred = null;
            vm.validatePasswordPromise = null;

            vm.tempNewPassword = "";
            vm.model = passwordData;
            vm.passwordConfig = passwordConfig;
            passwordConfig.setMethodsSrc(vm);

            vm.showExpireMsg = userModel.checkUserToken();
            var clearpw = setTimeout(vm.clearNewPasswordFields, 500);
         };

        vm.clearNewPasswordFields = function() {
            vm.model.createPassword = "";
            vm.model.confirmPassword = "";
            vm.passwordData.createPassword = "";
            vm.passwordData.confirmPassword = "";
        };

        vm.displayErrorMsg = function () {
            return vm.changePasswordForm.$submitted;
        };

        vm.changePassword = function () {
            vm.changePasswordForm.$setSubmitted();
            if (vm.changePasswordForm.$valid) {
                vm.toggleSaving();

                changePasswordSvc.save(vm.model)
                    .then(vm.checkResponse)
                    .finally(vm.toggleSaving);
            }
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
                vm.redirectToSecurityQuestions();
            }
            else {
                vm.errorMessages = data.errorReason;
            }
        };

        vm.validateChecklist = function (modelValue) {
            return userPasswordState.isPasswordValid(modelValue);
        };

        vm.validateNoUsername = function (modelValue) {
            var username = userModel.getEnterpriseUserName().toLowerCase(),
                smallModel = modelValue.toLowerCase();
            return username != smallModel;
        };

        vm.validateConfirmPassword = function (modelValue) {
            vm.tempNewPassword = modelValue;
            vm.changePasswordForm.confirmPassword.$validate();
        };

        vm.validatePassword = function (modelValue) {
            if (!modelValue && modelValue.length === 0) {
                return $q.resolve(); //consider empty model valid
            }

            if (vm.validatePasswordPromise) {
                vm.validatePasswordPromise.$cancelRequest();
            }

            vm.validatePasswordDeferred = $q.defer();
            vm.validatePasswordPromise = changePasswordSvc.validatePassword(modelValue, vm.validatePasswordCallback);

            return vm.validatePasswordDeferred.promise;
        };

        vm.validatePasswordCallback = function (response) {
            if (response.isSuccess) {
                vm.validatePasswordDeferred.resolve();
            }
            else {
                vm.setRequirementsError(response.errorReason);
                vm.validatePasswordDeferred.reject();
            }
        };

        vm.confirmValidPassword = function (modelValue) {
            var newPwd = passwordData.createPassword; //passwordModel.getCreatePassword();
            return vm.tempNewPassword === modelValue;
        };

        vm.setRequirementsError = function (errorMsg) {
            passwordConfig.setErrorMessage("createPassword", "minRequirements", errorMsg);
        };

        vm.setRequirementsError = function (errorMsg) {
            passwordConfig.setErrorMessage("createPassword", "minRequirements", errorMsg);
        };

        vm.redirectToSecurityQuestions = function () {
            $state.go("security-questions", $stateParams, {
                location: "replace"
            });
        };

        vm.cancelSetup = function () {
            userModel.reset();
            $window.location.replace("/home/");
        };

        vm.destroy = function () {
            vm.destroyCtrl();
            vm.destroyCtrl = undefined;

            userPasswordState.reset();
            vm.formState.password = undefined;

            passwordModel.reset();
            vm.changePasswordForm = undefined;

            vm.validatePasswordDeferred = undefined;
            vm.validatePasswordPromise = undefined;

            vm.tempNewPassword = undefined;
            vm.model = undefined;
            vm.passwordConfig = undefined;
            vm.errorMessages = undefined;
            vm.showExpireMsg = undefined;
            vm = undefined;
        };


        vm.init();
    };

    angular
        .module("new-user")
        .controller("ChangePwdController", [
            "$scope",
            "$q",
            "$state",
            "$stateParams",
            "$window",
            "rpCookie",
            "changePasswordModel",
            "userPasswordState",
            "changePasswordSvc",
            "userModel",
            "userPasswordConfig",
            "changePasswordData",
            ChangePwdController
        ]);

})(angular);

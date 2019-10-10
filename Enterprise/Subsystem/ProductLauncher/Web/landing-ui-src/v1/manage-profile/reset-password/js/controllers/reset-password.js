//  Reset Password Controller

(function (angular, undefined) {
    "use strict";

    function MpResetPasswordTabCtrl($q, $scope, $stateParams, tabsManager, model, formConfig, password, userProfileModel, user) {
        var vm = this;

        vm.init = function () {
            tabsManager.registerTab({
                id: "03",
                ctrl: vm
            });

            vm.model = model;
            vm.saveError = false;
            vm.formConfig = formConfig;
            formConfig.setMethodsSrc(vm);
            vm.state = tabsManager.getTabState("03");
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.formWatch = $scope.$watch("resetPasswordForm", vm.setForm);

            if ($stateParams.userId && userProfileModel.isProfileCardCaller()) {
                //means manage profile is accessed from edit user
                vm.editUserId = $stateParams.userId;
                vm.model.setUsername(userProfileModel.getUsername());
            }
            else {
                vm.model.setUsername(user.getUsername());
            }
        };

        // Setters

        vm.setForm = function (form) {
            if (form) {
                vm.form = form;
                vm.formWatch();
                $scope.rpTrackFormChanges.setData(model.getData());
            }
        };

        vm.setSubmitted = function () {
            $scope.resetPasswordForm.$setSubmitted();
        };

        // Actions

        vm.focusInvalidField = function () {
            $scope.focusInvalidField.focus();
        };

        vm.onCancel = function () {
            model.reset();
        };

        vm.onUpdate = function () {
            vm.updateDeferred = $q.defer();

            if (vm.editUserId) {
                password.reset(vm.editUserId, model.getReqData(), vm.onUpdateSuccess, vm.onUpdateError);
            }
            else {
                password.reset(null, model.getReqData(), vm.onUpdateSuccess, vm.onUpdateError);
            }

            return vm.updateDeferred.promise;
        };

        vm.onUpdateError = function (resp) {
            vm.saveError = true;
            vm.updateDeferred.reject();
        };

        vm.onUpdateSuccess = function (resp) {
            if (resp.isSuccess) {
                model.reset();
                vm.saveError = false;
                vm.clearFormTracker();
                vm.form.$setUntouched();
                vm.updateDeferred.resolve();
            }
            else {
                vm.saveError = true;
                vm.updateDeferred.reject();
                model.setErrorMsg(resp.errorReason);
            }
        };

        // Assertions

        vm.clearFormTracker = function () {
            var data = model.getData();
            $scope.rpTrackFormChanges.setData(data).reset();
        };

        vm.hasSaveError = function () {
            return vm.saveError;
        };

        vm.isDirty = function () {
            return $scope.rpTrackFormChanges.isDirty();
        };

        vm.isValid = function () {
            return vm.form.$valid;
        };

        vm.newPasswordMatchesCurrent = function () {
            return model.newPasswordMatchesCurrent();
        };

        vm.passwordIsValid = function () {
            return model.passwordIsValid();
        };

        vm.passwordCopyIsValid = function () {
            return model.passwordCopyIsValid();
        };

        vm.destroy = function () {
            model.reset();
            vm.destWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("MpResetPasswordTabCtrl", [
            "$q",
            "$scope",
            "$stateParams",
            "mpTabsManager",
            "mpResetPasswordTabModel",
            "mpResetPasswordTabFormConfig",
            "mpResetPasswordSvc",
            "userProfileModel",
            "userSessionModel",
            MpResetPasswordTabCtrl
        ]);
})(angular);

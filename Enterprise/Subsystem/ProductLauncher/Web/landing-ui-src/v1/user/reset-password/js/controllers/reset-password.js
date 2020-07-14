//  Reset Password Controller

(function(angular, undefined) {
    "use strict";

    function ResetPasswordCtrl($scope, $q, $filter, $params, model, timeout, tabs, formConfig, resetsvc, settempsvc, helpData, security, userDetModel, impersonate, session, PasswordPolicySvc) {
        var vm = this,
            lang = $filter("resetPasswordText");

        vm.init = function() {
            vm.formState = {
                password: null,
                isSaving: false
            };
            vm.model = model;
            vm.security = security;
            vm.formConfig = formConfig.setMethodsSrc(vm);

            vm.validatePasswordPolicy();

            vm.register();
            vm.setControlsState();
            vm.activeWatch = $scope.$watch(vm.isReady, vm.setControlsStateImp);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            var clearpw = setTimeout(vm.clearNewPasswordFields, 800);
        };

        vm.isReady = function() {
            return impersonate.isReady();
        };

        // Setters
        vm.clearNewPasswordFields = function() {
            vm.model.data.newPassword = "";
            vm.model.data.newPasswordCopy = "";
        };

        vm.validatePasswordPolicy = function() {
            var records = session.getOrganization();
            var params = {
                    PartyId: records[0].partyId
            };
            PasswordPolicySvc.get(params, vm.onResponseReady, vm.setDataErr);
        };

        vm.onResponseReady = function (resp) {
                var settings = resp.data;
                vm.formState.password = model.init(settings);
        };

        vm.setDataErr = function (resp) {
            alert(resp);
        };

        vm.setControlsState = function() {
            var allowed = security.isAllowed("editSelf") ||
                security.isAllowed("editOther") ||
                security.isAllowed("editUser") ? true : false;

            if (allowed) {
                formConfig.setControlsDisabledState(false);
            } else {
                formConfig.setControlsDisabledState(true);
            }

            return vm;
        };

        vm.setControlsStateImp = function() {

            if (impersonate.isUserImpersonated() && (session.getRealPageId() === $params.realPageId)) {
                formConfig.setControlsDisabledState(true);
            }
        };

        // Actions

        vm.focusInvalidField = function() {
            $scope.focusInvalidField.focus();
        };

        vm.onSaveError = function() {
            vm.saveReq.reject({
                success: false,
                tabName: "resetPassword"
            });
        };

        vm.onSaveSuccess = function(resp) {
            if (resp.isSuccess) {
                vm.saveReq.resolve({
                    success: true,
                    tabName: "resetPassword"
                });
            } else {
                vm.saveReq.reject({
                    success: false,
                    tabName: "resetPassword"
                });
                model.setErrorMsg(resp.errorReason);
            }
        };

        vm.onTabActive = function() {
            var helpWidget = document.querySelector('omnibar-unified-help');
            helpWidget.helpQuery = 'pg=ul-password&vr=40&scrver=350';
            vm.active = true;
            model.setActive();
            return vm;
        };

        vm.onTabInactive = function() {
            vm.active = false;
            return vm;
        };

        vm.register = function() {
            tabs.register({
                ctrl: vm,
                name: "resetPassword"
            });
        };


        vm.saveData = function() {
            var data = model.getData(),
                method = "save";

            data.realPageId = $params.realPageId;

            vm.saveReq = $q.defer();
            if (model.editingSelf()) {
                resetsvc[method](data, vm.onSaveSuccess, vm.onSaveError);
            } else {
                settempsvc[method](data, vm.onSaveSuccess, vm.onSaveError);
            }

            return vm.saveReq.promise;
        };

        vm.showErrors = function() {
            vm.resetPasswordForm.$setSubmitted();
            timeout($scope.focusInvalidField.focus, 100);
        };

        // Assertions

        vm.isDirty = function() {
            return model.isDirty();
        };

        vm.isEditingSelf = function() {
            return model.editingSelf();
        };

        vm.isValid = function() {
            return vm.isDirty() ? vm.resetPasswordForm.$valid : true;
        };

        // vm.hasAccess = function () {
        //     return security.isAllowed("editPassWord");
        // };

        vm.hasSaveFn = function() {
            return vm.isDirty() && vm.isValid();
        };

        vm.hasError = function() {
            return model.passwordCopyIsValid();
        };

        vm.newPasswordMatchesCurrent = function() {
            return model.newPasswordMatchesCurrent();
        };

        vm.passwordIsValid = function() {
            return model.passwordIsValid();
        };

        vm.passwordCopyIsValid = function() {
            return model.passwordCopyIsValid();
        };

        vm.destroy = function() {
            model.reset();
            vm.activeWatch();
            vm.destWatch();
            tabs.remove("resetPassword");
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ResetPasswordCtrl", [
            "$scope",
            "$q",
            "$filter",
            "$stateParams",
            "ResetPasswordModel",
            "timeout",
            "userTabsManager",
            "ResetPasswordFormConfig",
            "resetPasswordSvc",
            "setTempPasswordSvc",
            "rpGhHelpData",
            "routeSecurity",
            "userDetailsModel",
            "userImpersonated",
            "userSessionModel",
            "PasswordPolicySvc",
            ResetPasswordCtrl
        ]);
})(angular);

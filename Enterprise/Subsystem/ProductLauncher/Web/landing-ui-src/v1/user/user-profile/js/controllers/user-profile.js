//  Profile Tab Controller

(function(angular, undefined) {
    "use strict";

    function UserProfileCtrl($q, $scope, $stateParams, $filter, session, model, profileSvc, formConfig, pubsub, statusMsg, statusMsgModel, security, persona) {
        var vm = this,
            lang = $filter("userEditProfileText");

        vm.init = function() {
            vm.security = security;
            vm.saveError = false;
            vm.displayHeader = true;
            vm.displayDetails = false;
            vm.formConfig = formConfig;
            formConfig.setMethodsSrc(vm);
            vm.initUser();
            vm.model = model;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.formWatch = $scope.$watch("profileForm", vm.setForm);
            model.setPhoneNumberChangeCallback(vm.validatePhoneNumber);

        };

        // Setters

        vm.setForm = function(form) {
            if (form) {
                vm.form = form;
                vm.formWatch();
            }
        };

        vm.setSubmitted = function() {
            vm.form.$setSubmitted();
            return vm;
        };

        // Actions
        vm.displayPanelHeader = function() {
            vm.displayHeader = true;
            vm.displayDetails = false;
            pubsub.publish("up.user-details-disable", false);
        };

        vm.displayPanelDetails = function() {
            vm.displayHeader = false;
            vm.displayDetails = true;
            pubsub.publish("up.user-details-disable", true);
        };

        vm.editingSelf = function() {
            if (session.getRealPageId() === $stateParams.realPageId) {
                return true;
            }
            return false;
        };

        vm.focusInvalidField = function() {
            $scope.focusInvalidField.focus();
        };

        vm.cancel = function() {
            vm.displayPanelHeader();
        };

        vm.initUser = function() {
            profileSvc.get($stateParams.realPageId, vm.onDataReady);
        };

        vm.hasAccess = function() {
            var activity,
                allowed = true;

            if (session.getRealPageId() === $stateParams.realPageId) {
                activity = "editSelf";
            } else {
                activity = "editOther";
            }

            if (!security.isAllowed(activity)) {
                allowed = false;
            }

            if (persona.hasViewOnlySupportToolAccess()){
                allowed = false;
            }

            return allowed;
        };

        vm.onDataReady = function(resp) {
            model.setData(resp);
        };

        vm.update = function() {
            if (vm.form.$valid) {
                var data = model.getData();
                vm.updateDeferred = $q.defer();
                profileSvc.save($stateParams.realPageId, data, vm.onUpdateSuccess, vm.onUpdateError);
                return vm.updateDeferred.promise;
            }
        };

        vm.onUpdateError = function(resp) {
            vm.saveError = true;
            var data = "";
            vm.updateDeferred.reject();
        };

        vm.onUpdateSuccess = function(resp) {
            vm.saveError = false;
            var data = "";
            vm.form.$setUntouched();
            vm.updateDeferred.resolve();
            if (resp.status.success) {
                model.setData(resp);
                vm.displayPanelHeader();
            }

        };

        vm.showStatusMsg = function(data, errorCode) {
            statusMsgModel.setData({
                tabStatus: data,
                userExists: true,
                errorCode: errorCode
            });

            statusMsg.show();
        };
        // Assertions

        vm.hasSaveError = function() {
            return vm.saveError;
        };

        vm.isLoginNameContainsEmail = function() {
            return model.loginNameIsEmail();
        };

        vm.isValidPhonenumber = function() {
            return model.hasValidPhoneNumber();
        };

        vm.isDirty = function() {
            return $scope.rpTrackFormChanges.isDirty();
        };

        vm.isValid = function() {
            return vm.isDirty() ? vm.form.$valid : true;
        };

        vm.validatePhoneNumber = function() {
            var fieldNames = model.getPhoneNumberFieldNames();

            fieldNames.forEach(function(fieldName) {
                vm.form[fieldName].$validate();
            });
        };

        vm.addPhone = function() {
            if (vm.form.$valid) {
                vm.model.addPhone();
            }
        };

        vm.delPhone = function(item) {
            vm.model.delPhone(item);
        };

        vm.isEditingSelf = function() {
            return session.getRealPageId() === $stateParams.realPageId;
        };

        vm.addPhone = function () {
            if (vm.form.$valid) {
                vm.model.addPhone();
            }
        };

        vm.delPhone = function (item) {
            vm.model.delPhone(item);
        };

        vm.isEditingSelf = function () {
            return session.getRealPageId() === $stateParams.realPageId;
        };


        vm.destroy = function () {
            model.reset();
            vm.destWatch();
            vm = undefined;
            $scope = undefined;
            lang = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("UserProfileCtrl", [
            "$q",
            "$scope",
            "$stateParams",
            "$filter",
            "userSessionModel",
            "profileModel",
            "userProfileDataSvc",
            "profileFormConfig",
            "pubsub",
            "userReqStatusMsgModal",
            "userReqStatusMsgModel",
            "routeSecurity",
            "personaDetails",
            UserProfileCtrl
        ]);
})(angular);

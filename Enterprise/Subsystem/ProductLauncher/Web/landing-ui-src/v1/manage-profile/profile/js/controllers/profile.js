//  Profile Tab Controller

(function (angular, undefined) {
    "use strict";

    function MpProfileTabCtrl($q, $scope, $stateParams, user, tabsManager, model, profileSvc, formConfig) {
        var vm = this;

        vm.init = function () {
            tabsManager.registerTab({
                id: "01",
                ctrl: vm
            });

            vm.model = model;
            vm.saveError = false;
            vm.formConfig = formConfig;
            formConfig.setMethodsSrc(vm);
            vm.state = tabsManager.getTabState("01");
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.formWatch = $scope.$watch("profileForm", vm.setForm);
            model.setPhoneNumberChangeCallback(vm.validatePhoneNumber);

            if ($stateParams.userId) { //means manage profile is accessed from edit user
                vm.editUserId = $stateParams.userId;
            }
        };

        // Setters

        vm.setForm = function (form) {
            if (form) {
                vm.form = form;
                vm.formWatch();
            }
        };

        vm.setSubmitted = function () {
            vm.form.$setSubmitted();
            return vm;
        };

        // Actions

        vm.focusInvalidField = function () {
            $scope.focusInvalidField.focus();
        };

        vm.onCancel = function () {
            model.reset();
        };

        vm.onTabActive = function () {
            if (vm.editUserId) { //means manage profile is accessed from edit user
                profileSvc.get(vm.editUserId, vm.onDataReady);
            }
            else {
                profileSvc.get(null, vm.onDataReady);
            }
        };

        vm.onDataReady = function (resp) {
            model.setData(resp);
            $scope.rpTrackFormChanges.setData(resp.data);
        };

        vm.onUpdate = function () {
            var data = model.getData();
            vm.updateDeferred = $q.defer();
            vm.clearFormTracker(data);
            if (vm.editUserId) {
                profileSvc.save(vm.editUserId, data, vm.onUpdateSuccess, vm.onUpdateError);
            }
            else {
                profileSvc.save(null, data, vm.onUpdateSuccess, vm.onUpdateError);
            }
            return vm.updateDeferred.promise;
        };

        vm.onUpdateError = function (resp) {
            vm.saveError = true;
            vm.updateDeferred.reject();
        };

        vm.onUpdateSuccess = function (resp) {
            vm.saveError = false;
            vm.form.$setUntouched();
            vm.updateDeferred.resolve();
        };

        // Assertions

        vm.clearFormTracker = function(data) {
            $scope.rpTrackFormChanges.setData(data);
            $scope.rpTrackFormChanges.checkState();
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

        vm.validatePhoneNumber = function () {
            var fieldNames = model.getPhoneNumberFieldNames();

            fieldNames.forEach(function (fieldName) {
                vm.form[fieldName].$validate();
            });
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
        .controller("MpProfileTabCtrl", [
            "$q",
            "$scope",
            "$stateParams",
            "userSessionModel",
            "mpTabsManager",
            "mpProfileTabModel",
            "mpProfileTabDataSvc",
            "mpProfileTabFormConfig",
            MpProfileTabCtrl
        ]);
})(angular);

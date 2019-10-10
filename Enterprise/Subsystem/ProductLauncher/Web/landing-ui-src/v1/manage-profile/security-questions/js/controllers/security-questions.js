//  Security Questions Controller

(function (angular, undefined) {
    "use strict";

    function MpSecurityQuestionsTabCtrl($q, $scope, $stateParams, $filter, tabsManager, questionsList, questionModel) {
        var vm = this;

        vm.init = function () {
            tabsManager.registerTab({
                id: "02",
                ctrl: vm
            });

            vm.saveError = false;
            vm.questionsList = [];
            vm.state = tabsManager.getTabState("02");
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.formWatch = $scope.$watch("securityQuestionsForm", vm.setForm);

            if ($stateParams.userId) { //means manage profile is accessed from edit user
                vm.editUserId = $stateParams.userId;
            }
        };

        // Setters

        vm.setData = function (resp) {
            if(resp.status.success) {
                var qnsData = resp.data || [];

                qnsData.forEach(function (data) {
                    var question = questionModel(data);
                        question.setQuestionsFilter(vm.filterSelectedQuestions);
                    vm.questionsList.push(question);
                });

                $scope.rpTrackFormChanges.setData(qnsData);
            }
        };

        vm.setForm = function (form) {
            if (form) {
                vm.form = form;
                vm.formWatch();
            }
        };

        vm.setSubmitted = function () {
            vm.form.$setSubmitted();
        };

        // Getters

        vm.getUpdateData = function () {
            var list = [];

            vm.questionsList.forEach(function (item) {
                list.push(item.getData());
            });

            return list;
        };

        // Actions

        vm.destroyQuestionsList = function () {
            vm.questionsList.forEach(function (question) {
                question.destroy();
            });

            vm.questionsList = [];
        };

        vm.filterSelectedQuestions = function(currId, options) {
            var selectedOptions = [];

            angular.forEach(vm.questionsList, function(curr) {
                if(currId !== curr.getId()) {
                    selectedOptions.push(curr.getQuestionId());
                }
            });

            return $filter("filter")(options, vm.filterOptions.bind(null, selectedOptions));
        };

        vm.filterOptions = function(selectedOptions, curr) {
            if(selectedOptions.indexOf(curr.value) == -1) {
                return true;
            }
            return false;
        };

        vm.focusInvalidField = function () {
            $scope.focusInvalidField.focus();
        };

        vm.onCancel = function () {
            vm.destroyQuestionsList();
        };

        vm.onTabActive = function () {
            if (vm.editUserId) { //edit user manage profile
                questionsList.get(vm.editUserId, vm.setData);
            }
            else {
                questionsList.get(null, vm.setData);
            }
        };

        vm.onUpdate = function () {
            var data = vm.getUpdateData();
            vm.updateDeferred = $q.defer();
            vm.clearFormTracker(data);
            if (vm.editUserId) {
                questionsList.save(vm.editUserId, data, vm.onUpdateSuccess, vm.onUpdateError);
            }
            else {
                questionsList.save(null, data, vm.onUpdateSuccess, vm.onUpdateError);
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
            vm.destroyQuestionsList();
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

        vm.destroy = function () {
            vm.destWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("MpSecurityQuestionsTabCtrl", [
            "$q",
            "$scope",
            "$stateParams",
            "$filter",
            "mpTabsManager",
            "mpSecurityQuestionsList",
            "securityQuestionModel",
            MpSecurityQuestionsTabCtrl
        ]);
})(angular);

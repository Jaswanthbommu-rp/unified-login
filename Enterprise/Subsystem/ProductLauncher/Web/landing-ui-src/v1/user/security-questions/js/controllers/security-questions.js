//  Security Questions Controller

(function (angular, undefined) {
    "use strict";

    function SecurityQuestionsCtrl($q, $scope, $stateParams, $filter, tabsManager, questionsList, questionModel, timeout, session, helpData) {
        var vm = this;

        vm.init = function () {
            vm.saveError = false;
            vm.questionsList = [];
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.formWatch = $scope.$watch("userSecurityQuestionsForm", vm.setForm);
            vm.register();
            vm.getQuestionsData();
        };

        vm.register = function () {
            tabsManager.register({
                ctrl: vm,
                name: "securityQuestions"
            });
        };
        // Setters

        vm.setData = function (resp) {
            if (resp.status.success) {
                var qnsData = resp.data || [];

                qnsData.forEach(function (data) {
                    var question = questionModel(data);
                    question.setQuestionsFilter(vm.filterSelectedQuestions);
                    vm.questionsList.push(question);
                });
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
        vm.getQuestionsData = function () {
            if ($stateParams.realPageId) { //edit user manage profile
                vm.questionsData = questionsList.get($stateParams.realPageId, vm.setData);
            }
            else {
                vm.questionsData = questionsList.get(null, vm.setData);
            }

            return vm;
        };

        vm.getUpdateData = function () {
            var list = [];

            vm.questionsList.forEach(function (item) {
                if (item.data.questionId > 0) {
                    list.push(item.getData());
                }
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

        vm.filterSelectedQuestions = function (currId, options) {
            var selectedOptions = [];

            angular.forEach(vm.questionsList, function (curr) {
                if (currId !== curr.getId()) {
                    selectedOptions.push(curr.getQuestionId());
                }
            });

            return $filter("filter")(options, vm.filterOptions.bind(null, selectedOptions));
        };

        vm.filterOptions = function (selectedOptions, curr) {
            if (selectedOptions.indexOf(curr.value) == -1) {
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
            var helpWidget = document.querySelector('omnibar-unified-help');            
            helpWidget.helpQuery = 'pg=ul-securityQuestions&vr=40&scrver=350';
            vm.active = true;
            return vm;
        };

        vm.onTabInactive = function () {
            vm.active = false;
            return vm;
        };

        vm.saveData = function () {
            var data = vm.getUpdateData();

            vm.updateDeferred = $q.defer();
            questionsList.save($stateParams.realPageId, data, vm.onUpdateSuccess, vm.onUpdateError);

            return vm.updateDeferred.promise;
        };

        vm.showErrors = function () {
            vm.form.$setSubmitted();
            timeout($scope.focusInvalidField.focus, 100);
        };

        vm.onUpdateError = function (resp) {
            vm.saveError = true;
            vm.updateDeferred.reject({
                success: false,
                tabName: "securityQuestions"
            });
        };

        vm.onUpdateSuccess = function (resp) {
            vm.saveError = false;
            vm.form.$setUntouched();

            if (!resp.isError) {
                vm.updateDeferred.resolve({
                    success: true,
                    tabName: "securityQuestions"
                });
            }
            else {
                vm.updateDeferred.reject({
                    success: false,
                    tabName: "securityQuestions"
                });
            }
        };

        vm.editingSelf = function () {
            return session.getRealPageId() === $stateParams.realPageId;
        };
        // Assertions

        vm.hasError = function () {
            return vm.saveError;
        };

        vm.hasSaveFn = function () {
            var data = vm.getUpdateData();
            return vm.editingSelf() && vm.isValid() && data.length > 0;
        };

        vm.isDirty = function () {
            return vm.form.$dirty;
        };

        vm.isValid = function () {
            return vm.isDirty() ? vm.form.$valid : true;
        };

        vm.destroy = function () {
            vm.destWatch();
            tabsManager.remove("securityQuestions");
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("SecurityQuestionsCtrl", [
            "$q",
            "$scope",
            "$stateParams",
            "$filter",
            "userTabsManager",
            "securityQuestionsList",
            "userSecurityQuestionModel",
            "timeout",
            "userSessionModel",
            "rpGhHelpData",
            SecurityQuestionsCtrl
        ]);
})(angular);

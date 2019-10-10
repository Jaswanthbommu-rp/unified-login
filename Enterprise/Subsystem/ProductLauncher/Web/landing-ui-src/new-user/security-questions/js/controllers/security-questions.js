//  Security Questions Controller

(function (angular) {
    "use strict";

    function SecurityQuestionsCtrl($scope, $state, $stateParams, $window, $filter, userModel,
            securityQuestionsFormModel, securityQuestionsSvc, securityQuestionOptionsSvc, questionModel) {

        var vm = this;

        vm.init = function () {
        	vm.destroyCtrl = $scope.$on("$destroy", vm.destroy);

            vm.securityQuestions = 3; //TODO should eventually come from product requirements
            vm.errorMsg = "";
            vm.questionsList = [];

            vm.setQuestionData();
        };

        vm.setQuestionData = function () {
            for (var a = 0; a < vm.securityQuestions; a++) {
                var question = questionModel();
                    question.setQuestionsFilter(vm.filterSelectedQuestions);
                vm.questionsList.push(question);
            }
        };

        vm.submitSecurityQuestions = function (form) {
            if (form.$valid) {
                securityQuestionsFormModel.setSubmitBtnDisabled();
                securityQuestionsSvc.save(vm.questionsList)
                    .then(vm.processSecurityQuestionsForm)
                    .finally(vm.setSubmitBtnEnabled);
            } else {
                form.$setSubmitted();
            }
        };

        vm.processSecurityQuestionsForm = function (data) {
            if (data.isSuccess === true && data.isError === false) {
                // vm.redirectToStartProfile(); // Disabling profile page for now
                 vm.redirectToLogin();
            } else if (data.isError === true) {
                vm.errorMsg = data.errorReason;
            } else {
                vm.displayError();
            }
        };

        vm.redirectToLogin = function () {
            $window.location.replace("/home/?msgId=200");
        };

        vm.displayError = function () {
            vm.errorMsg = $filter("securityQuestionsText")("errorMsgs.generic");
        };

        vm.redirectToStartProfile = function () {
            $state.go("start-profile", $stateParams, { location: "replace" });
        };

        vm.setSubmitBtnEnabled = function () {
            securityQuestionsFormModel.setSubmitBtnDisabled(false);
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

        vm.cancelSetup = function () {
            userModel.reset();
            
            $window.location.replace("/home/");
        };

        vm.destroy = function () {
            securityQuestionsFormModel.reset();

        	vm.destroyCtrl();
        	vm = undefined;
        };

        vm.init();
    }

    angular
        .module("new-user")
        .controller("SecurityQuestionsCtrl", [
        	"$scope",
        	"$state",
            "$stateParams",
            "$window",
            "$filter",
        	"userModel",
        	"securityQuestionsFormModel",
        	"securityQuestionsSvc",
        	"securityQuestionOptionsSvc",
            "securityQuestionModel",
        	SecurityQuestionsCtrl
        ]);
})(angular);

//  Forgot Password Controller

(function(angular) {
    "use strict";

    function ForgotPasswordCtrl($scope, $filter, forgotPasswordModel, userModel, layoutModel, forgotPasswordSvc) {
        var vm = this;

        vm.init = function() {
            vm.model = forgotPasswordModel;
            vm.questions = userModel.getSecurityQuestions();
            vm.forgotPasswordCtrlWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.setLoginState = function() {
            userModel.reset();
            layoutModel.setActiveState("login");

            return vm;
        };

        vm.submit = function(form) {
            if (form.$valid) {
                forgotPasswordModel.setSubmitBtnDisabled();
                forgotPasswordSvc.submitAnswers(vm.model.getAnswers())
                    .then(vm.checkResponse, vm.displayError)
                    .finally(vm.setSubmitBtnEnabled);
            } else {
                form.$setSubmitted();
            }
        };

        vm.setSubmitBtnEnabled = function() {
            forgotPasswordModel.setSubmitBtnDisabled(false);
        };

        vm.checkResponse = function(data) {
            if (!data.isError && data.isAnswersCorrect === true && data.correctAnswerToken) {
                userModel.setCorrectAnswerToken(data.correctAnswerToken);
                vm.redirectToChangePassword();
            } else {
                vm.model.setError(data.errorReason);
                vm.model.setDefaults();
                if (data.securityQuestions) {
                    userModel.setSecurityQuestions(data.securityQuestions);
                    vm.questions = userModel.getSecurityQuestions();
                }
            }
        };

        vm.displayError = function(error) {
            vm.model.setError(
                $filter("forgotPassword")("forgot_system_err_contact_admin"));
        };

        vm.redirectToChangePassword = function() {
            userModel.clearSecurityQuestions();
            layoutModel.setActiveState("change-password");
        };

        vm.destroy = function() {
            vm.forgotPasswordCtrlWatch();
            vm.model.reset();
            vm = undefined;
        };

        vm.init();
    }

    angular
        .module("identity")
        .controller("ForgotPasswordCtrl", [
            "$scope",
            "$filter",
            "forgotPasswordModel",
            "userModel",
            "layoutModel",
            "forgotPasswordSvc",
            ForgotPasswordCtrl
        ]);
})(angular);

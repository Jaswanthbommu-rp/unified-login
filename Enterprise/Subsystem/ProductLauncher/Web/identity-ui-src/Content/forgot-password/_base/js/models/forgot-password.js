//  Forgot Password Model

(function(angular) {
    "use strict";

    function factory(userModel) {
        var model = {},
            formDefault,
            error;

        model.form = {};

        formDefault = {
            securityAnswer1: "",
            securityAnswer2: "",
            submitBtnDisabled: false
        };

        error = {
            state: false,
            message: ""
        };

        model.init = function() {
            model.reset();

            return model;
        };

        model.getAnswers = function() {
            var answers;
            answers = [{
                questionId: userModel.getSecurityQuestionId(0),
                answer: model.form.securityAnswer1,
            }, {
                questionId: userModel.getSecurityQuestionId(1),
                answer: model.form.securityAnswer2,
            }];

            return answers;
        };

        model.setError = function(errorMsg) {
            model.error.state = true;
            model.error.message = errorMsg;

            return model;
        };

        model.clearError = function() {
            model.error = angular.extend({}, error);

            return model;
        };

        model.setDefaults = function() {
            model.form = angular.extend({}, formDefault);

            return model;
        };

        model.setSubmitBtnDisabled = function(state) {
            model.form.submitBtnDisabled = state === undefined || state === true ? true : false;

            return model;
        };

        model.reset = function() {
            model.setDefaults();
            model.clearError();

            return model;
        };

        return model.init();
    }

    angular
        .module("identity")
        .factory("forgotPasswordModel", [
            "userModel",
            factory
        ]);
})(angular);

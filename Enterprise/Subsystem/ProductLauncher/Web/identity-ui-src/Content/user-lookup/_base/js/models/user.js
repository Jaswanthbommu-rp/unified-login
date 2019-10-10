//  User Model

(function(angular) {
    "use strict";

    function factory() {
        var model = {},
            userDefault,
            securityQuestionsDefault;

        securityQuestionsDefault = [{
            securityQuestionId: "",
            question: ""
        }, {
            securityQuestionId: "",
            question: ""
        }];

        userDefault = {
            enterpriseLoginName: "",
            activityToken: "",
            correctAnswerToken: "",
            securityQuestions: securityQuestionsDefault
        };

        model.init = function() {
            model.user = angular.copy(userDefault);

            return model;
        };

        model.getEnterpriseLoginName = function() {
            return model.user.enterpriseLoginName;
        };

        model.setEnterpriseLoginName = function(username) {
            model.user.enterpriseLoginName = username;

            return model;
        };

        model.getActivityToken = function() {
            return model.user.activityToken;
        };

        model.setActivityToken = function(activityToken) {
            model.user.activityToken = activityToken;

            return model;
        };

        model.getCorrectAnswerToken = function() {
            return model.user.correctAnswerToken;
        };

        model.setCorrectAnswerToken = function(correctAnswerToken) {
            model.user.correctAnswerToken = correctAnswerToken;

            return model;
        };

        model.getSecurityQuestionId = function(index) {
            return model.user.securityQuestions[index].questionId;
        };

        model.getSecurityQuestions = function() {
            return model.user.securityQuestions;
        };

        model.setSecurityQuestions = function(securityQuestions) {
            model.user.securityQuestions = securityQuestions;

            return model;
        };

        model.clearSecurityQuestions = function() {
            model.user.securityQuestions = angular.copy(securityQuestionsDefault);

            return model;
        };

        model.reset = function() {
            model.user = angular.copy(userDefault);

            return model;
        };

        return model.init();
    }

    angular
        .module("identity")
        .factory("userModel", [
            factory
        ]);
})(angular);

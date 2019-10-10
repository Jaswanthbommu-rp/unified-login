//  Security Question Model

(function (angular, undefined) {
    "use strict";

    function factory($filter, eventStream, inputConfig, menuConfig) {
        var index = 0;

        function SecurityQuestionModel() {
            var s = this;
            index++;
            s.init();
        }

        var p = SecurityQuestionModel.prototype;

        p.init = function () {
            var s = this;

            s.onChange = eventStream();

            s.index = index;

            s.data = {
                questionId: "",
                customQuestion: "",
                answer: ""
            };

            s.questionConfig = menuConfig.get({
                required: true,
                id: "question-" + s.index,
                fieldName: "question-" + s.index,
                onChange: s.changeCallback.bind(s),
                errorMsgs: [
                    {
                        name: "required",
                        text: $filter("securityQuestionsText")("errorMsgs.questionConfig.required")
                    }
                ]
            });

            s.customQuestionConfig = inputConfig({
                id: "custom-question-" + s.index,
                fieldName: "custom-question-" + s.index,
                errorMsgs: [
                    {
                        name: "isRequired",
                        text: $filter("securityQuestionsText")("errorMsgs.customQuestionConfig.required")
                    }
                ],
                validators: {
                    isRequired: s.requireCustomQuestion.bind(s)
                }
            });

            s.answerConfig = inputConfig({
                required: true,
                id: "answer-" + s.index,
                fieldName: "answer-" + s.index,
                validators: {
                    checkChars: s.checkChars.bind(s)
                },
                errorMsgs: [
                    {
                        name: "required",
                        text: $filter("securityQuestionsText")("errorMsgs.answerConfig.required")
                    },
                    {
                        name: "checkChars",
                        text: $filter("securityQuestionsText")("errorMsgs.answerConfig.checkChars")
                    }
                ]
            });
        };

        p.changeCallback = function () {
            var s = this;
            s.onChange.publish();
            return s;
        };

        p.getData = function () {
            var s = this;
            return {
                answer: s.data.answer,
                questionId: s.data.questionId
            };
        };

        p.getId = function () {
            var s = this;
            return s.index;
        };

        p.getQuestionId = function() {
            var s = this;
            return s.data.questionId;
        };

        p.getRawData = function () {
            var s = this;
            return s.data;
        };

        p.requireCustomQuestion = function () {
            var s = this;
            return s.data.questionId == -1;
        };

        p.setData = function (data) {
            var s = this;
            angular.extend(s.data, data || {});
            return s;
        };

        p.setQuestionsFilter = function(callback) {
            var s = this;
            s.questionConfig.setOptionsFilter(callback.bind(null, s.index));
            return s;            
        };

        p.isCustom = function () {
            var s = this;
            return s.data.questionId == -1;
        };

        p.checkChars = function (data) {
            return !data.match(/\*/);
        };

        p.destroy = function () {
            var s = this;
            s.onChange.destroy();
            s.questionConfig.destroy();

            s.answerConfig = undefined;
            s.questionConfig = undefined;
            s.customQuestionConfig = undefined;
        };

        return function (data) {
            return (new SecurityQuestionModel()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("securityQuestionModel", [
            "$filter",
            "eventStream",
            "rpFormInputTextConfig",
            "securityQuestionsMenuConfig",
            factory
        ]);
})(angular);

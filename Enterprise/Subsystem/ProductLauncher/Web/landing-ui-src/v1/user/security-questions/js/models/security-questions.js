//  User Security Question Model

(function (angular, undefined) {
    "use strict";

    function factory($params, session, eventStream, inputConfig, menuConfig) {
        var index = 0;

        function UserSecurityQuestionModel() {
            var s = this;
            index++;
            s.init();
        }

        var p = UserSecurityQuestionModel.prototype;

        p.init = function () {
            var s = this;

            s.onChange = eventStream();

            s.index = index;

            s.data = {
                questionId: "",
                customQuestion: "",
                answer: "**********"
            };

            if (session.getRealPageId() === $params.realPageId) {
                s.questionConfig = menuConfig.get({
                    required: true,
                    id: "question-" + s.index,
                    fieldName: "question-" + s.index,
                    readonly: false,
                    onChange: s.changeCallback.bind(s),
                    errorMsgs: [
                        {
                            name: "required",
                            text: "Security question is required"
                        }
                    ]
                });

                s.answerConfig = inputConfig({
                    required: true,
                    id: "answer-" + s.index,
                    fieldName: "answer-" + s.index,
                    readonly: false,
                    validators: {
                        checkChars: s.checkChars.bind(s)
                    },
                    errorMsgs: [
                        {
                            name: "required",
                            text: "Security question answer is required"
                    },
                        {
                            name: "checkChars",
                            text: "Security question answer is not valid"
                    }
                ]
                });
            }
            else {
                s.questionConfig = menuConfig.get({
                    required: true,
                    id: "question-" + s.index,
                    fieldName: "question-" + s.index,
                    readonly: true,
                    onChange: s.changeCallback.bind(s),
                    errorMsgs: [
                        {
                            name: "required",
                            text: "Security question is required"
                    }
                ]
                });

                s.answerConfig = inputConfig({
                    required: true,
                    id: "answer-" + s.index,
                    fieldName: "answer-" + s.index,
                    readonly: true,
                    validators: {
                        checkChars: s.checkChars.bind(s)
                    },
                    errorMsgs: [
                        {
                            name: "required",
                            text: "Security question answer is required"
                    },
                        {
                            name: "checkChars",
                            text: "Security question answer is not valid"
                    }
                ]
                });
            }

            s.customQuestionConfig = inputConfig({
                id: "custom-question-" + s.index,
                fieldName: "custom-question-" + s.index,
                errorMsgs: [
                    {
                        name: "isRequired",
                        text: "Custom question is required",
                    }
                ],
                validators: {
                    isRequired: s.requireCustomQuestion.bind(s)
                }
            });

        };

        p.changeCallback = function () {
            var s = this;
            s.onChange.publish();
            return s;
        };

        p.editingSelf = function () {
            var s = this;
            return session.getRealPageId() === $params.realPageId;
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

        p.getQuestionId = function () {
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

            s.data = data || {};

            if (s.data.questionId === 0) {
                s.data.questionId = "";
            }
            else {
                s.data.answer = "**********";
            }

            return s;
        };

        p.setQuestionsFilter = function (callback) {
            var s = this;
            s.questionConfig.setOptionsFilter(callback.bind(null, s.index));
            return s;
        };

        p.isCustom = function () {
            var s = this;
            return s.data.questionId == -1;
        };

        p.checkChars = function (data) {
            return !!data && !data.match(/\*/);
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
            return (new UserSecurityQuestionModel()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("userSecurityQuestionModel", [
            "$stateParams",
            "userSessionModel",
            "eventStream",
            "rpFormInputTextConfig",
            "userSecurityQuestionsMenuConfig",
            factory
        ]);
})(angular);

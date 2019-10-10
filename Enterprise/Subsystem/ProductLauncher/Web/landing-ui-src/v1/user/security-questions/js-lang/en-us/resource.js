(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("user.securityQuestions");

        bundle.set({
            "label.instructions": "Please select at least 3 security questions in case you need to verify your account at a later date.",

            "label.question": "Question",
            "label.customQuestion": "Custom Question",
            "label.answer": "Answer",

            "errorMsgs.generic": "A system error has occurred. Please contact your system administrator.",
            "errorMsgs.answerConfig.required": "Security question answer is required",
            "errorMsgs.answerConfig.checkChars": "Security question answer is not valid",
            "errorMsgs.questionConfig.required": "Security question is required",
            "errorMsgs.customQuestionConfig.required": "Security custom question is required",

            "option.default": "Select Security Question"
        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();

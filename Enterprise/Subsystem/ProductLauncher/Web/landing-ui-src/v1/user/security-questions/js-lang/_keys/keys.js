(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [
            "label.instructions",

            "label.question",
            "label.customQuestion",
            "label.answer",

            "errorMsgs.generic",
            "errorMsgs.answerConfig.required",
            "errorMsgs.answerConfig.checkChars",
            "errorMsgs.questionConfig.required",
            "errorMsgs.customQuestionConfig.required",

            "option.default"
        ];

        appLangKeys.app("user.securityQuestions").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();

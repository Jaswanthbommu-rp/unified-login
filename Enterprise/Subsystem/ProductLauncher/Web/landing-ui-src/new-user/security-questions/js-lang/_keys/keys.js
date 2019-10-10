(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [
            "label.instructions",

            "label.question",
            "label.customQuestion",
            "label.answer",

            "label.save",
            "label.cancel",

            "errorMsgs.generic",
            "errorMsgs.answerConfig.required",
            "errorMsgs.answerConfig.checkChars",
            "errorMsgs.questionConfig.required",
            "errorMsgs.customQuestionConfig.required",
            
            "option.default"
        ];

        appLangKeys.app("securityQuestions").set(keys);
    }

    angular
        .module("new-user")
        .config(["appLangKeysProvider", config]);
})();

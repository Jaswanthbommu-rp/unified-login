//  Security Questions Menu Config

(function (angular, undefined) {
    "use strict";

    function UserSecurityQuestionsMenuConfig(menuConfig, optionsSvc) {
        var index = 0,
            svc = this;

        svc.get = function (data) {
            index++;

            data = angular.extend({
                nameKey: "question",
                valueKey: "questionId",
                id: "question-" + index,
                fieldName: "question-" + index
            }, data || {});

            var config = menuConfig(data);
            optionsSvc.get(function (options) {
                config.setOptions(options);
            });
            return config;
        };
    }

    angular
        .module("settings")
        .service("userSecurityQuestionsMenuConfig", [
            "rpFormSelectMenuConfig",
            "userSecurityQuestionOptions",
            UserSecurityQuestionsMenuConfig
        ]);
})(angular);

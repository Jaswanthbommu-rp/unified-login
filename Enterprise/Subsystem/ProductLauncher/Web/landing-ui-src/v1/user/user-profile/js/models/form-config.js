//  Profile Form Config

(function (angular, undefined) {
    "use strict";

    function factory(baseFormConfig, jobTitle, inputTextConfig, contactMethod, regex) {
        var model = baseFormConfig();

        model.contactMethod = contactMethod.get({
            onChange: model.getMethod("validatePhoneNumber")
        });

        model.jobTitle = jobTitle.get();

        model.title = inputTextConfig({
            maxlength: 50
        });

        model.secondaryEmail = inputTextConfig({
            required: false,
            // dataType: "email",
            pattern: regex.email,
            errorMsgs: [
                {
                    name: "email",
                    text: "Email should be a valid email address"
                }
            ]
        });

        return model;
    }

    angular
        .module("settings")
        .factory("profileFormConfig", [
            "baseFormConfig",
            "jobTitleMenuConfig",
            "rpFormInputTextConfig",
            "contactMethodMenuConfig",
            "regex",
            factory
        ]);
})(angular);

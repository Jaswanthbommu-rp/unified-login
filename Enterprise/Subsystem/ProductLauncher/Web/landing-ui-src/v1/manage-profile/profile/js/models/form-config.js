//  Profile Tab Form Config

(function (angular, undefined) {
    "use strict";

    function factory(baseFormConfig, jobTitle, inputTextConfig, contactMethod) {
        var model = baseFormConfig();

        model.firstName = inputTextConfig({
            required: true,
            maxlength: 50,
            errorMsgs: [
                {
                    name: "required",
                    text: "First name is required"
                }
            ]
        });

        model.middleName = inputTextConfig({
            maxlength: 50
        });

        model.lastName = inputTextConfig({
            required: true,
            maxlength: 50,
            errorMsgs: [
                {
                    name: "required",
                    text: "Last name is required"
                }
            ]
        });

        model.contactMethod = contactMethod.get({
            onChange: model.getMethod("validatePhoneNumber")
        });

        model.jobTitle = jobTitle.get();

        model.title = inputTextConfig({
            maxlength: 50
        });

        return model;
    }

    angular
        .module("settings")
        .factory("mpProfileTabFormConfig", [
            "baseFormConfig",
            "jobTitleMenuConfig",
            "rpFormInputTextConfig",
            "contactMethodMenuConfig",
            factory
        ]);
})(angular);

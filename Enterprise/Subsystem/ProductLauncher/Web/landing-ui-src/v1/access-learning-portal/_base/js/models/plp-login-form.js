//  Profile Tab Form Config

(function (angular, undefined) {
    "use strict";

    function factory(baseFormConfig, inputTextConfig) {
        var model = baseFormConfig();

        model.emailAddress = inputTextConfig({
            required: true,
            dataType: "email",
            errorMsgs: [{
                    name: "required",
                    text: "Email address is required"
                },
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
        .factory("plpLoginFormConfig", [
            "baseFormConfig",
            "rpFormInputTextConfig",
            factory
        ]);
})(angular);
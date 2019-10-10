//  Login Form Config

(function (angular, undefined) {
    "use strict";

    function factory(baseFormConfig, inputTextConfig) {
        var model = baseFormConfig();

        model.emailAddress = inputTextConfig({
            required: true,
            errorMsgs: [{
                    name: "required",
                    text: "Email address is required"
                }
            ]
        });

        return model;
    }

    angular
        .module("settings")
        .factory("elmsLoginFormConfig", [
            "baseFormConfig",
            "rpFormInputTextConfig",
            factory
        ]);
})(angular);

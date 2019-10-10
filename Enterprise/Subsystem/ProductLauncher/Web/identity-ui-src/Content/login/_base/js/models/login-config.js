//  Login Page Form Config

(function (angular) {
    "use strict";

    function factory($filter, baseFormConfig, inputConfig) {
		var model = baseFormConfig();

	    model.username = inputConfig({
            id: "username",
            fieldName: "username",
            maxlength: 100,
            required: true,
            errorMsgs: [
                {
                    name: "required",
                    text: $filter("loginText")("login_username_req")
                }
            ],

            modelOptions: {
                allowInvalid: true
            },

            onBlur: model.getMethod("determineIdentityProvider")
        });

        model.password = inputConfig({
            id: "password",
            fieldName: "password",
            maxlength: 100,
            required: true,
            dataType: "password",
            trimInput: false,
            errorMsgs: [
                {
                    name: "required",
                    text: $filter("loginText")("login_password_req")
                }
            ],
            modelOptions: {
                allowInvalid: true
            },
            autocomplete: "new-password"
        });

	    return model;
    }

    angular
        .module("identity")
        .factory("loginConfig", [
                "$filter",
        		"baseFormConfig",
        		"rpFormInputTextConfig",
        		factory
    		]);
})(angular);

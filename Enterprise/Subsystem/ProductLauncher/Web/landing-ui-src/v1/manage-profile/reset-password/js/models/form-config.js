//  Reset Password Form Config

(function(angular, undefined) {
    "use strict";

    function factory($filter, baseFormConfig, inputTextConfig) {
        var model = baseFormConfig();

        model.oldPassword = inputTextConfig({
            required: true,
            fieldName: "oldPassword",
            dataType: "password",

            modelOptions: {
                allowInvalid: true,
                updateOn: "default"
            },

            trimInput: false,

            errorMsgs: [{
                name: "required",
                text: $filter("resetPasswordText")("errorMsgs.currentPassword.required")
            }]
        });

        model.newPassword = inputTextConfig({
            required: true,
            fieldName: "newPassword",
            dataType: "password",

            modelOptions: {
                allowInvalid: true,
                updateOn: "default"
            },

            trimInput: false,

            asyncValidators: {
                isValid: model.getMethod("passwordIsValid"),
                isSameAsCurrent: model.getMethod("newPasswordMatchesCurrent")
            },

            errorMsgs: [{
                name: "required",
                text: $filter("resetPasswordText")("errorMsgs.password.required")
            }, {
                name: "isValid",
                text: $filter("resetPasswordText")("errorMsgs.password.incompleteRequirements")
            }, {
                name: "isSameAsCurrent",
                text: $filter("resetPasswordText")("errorMsgs.password.notSameAsCurrent")
            }]
        });

        model.newPasswordCopy = inputTextConfig({
            required: true,
            fieldName: "newPasswordCopy",
            dataType: "password",

            modelOptions: {
                allowInvalid: true,
                updateOn: "default"
            },

            trimInput: false,

            asyncValidators: {
                isMatch: model.getMethod("passwordCopyIsValid")
            },

            errorMsgs: [{
                name: "required",
                text: $filter("resetPasswordText")("errorMsgs.confirmPassword.required")
            }, {
                name: "isMatch",
                text: $filter("resetPasswordText")("errorMsgs.confirmPassword.mismatch")
            }]
        });

        return model;
    }

    angular
        .module("settings")
        .factory("mpResetPasswordTabFormConfig", [
            "$filter",
            "baseFormConfig",
            "rpFormInputTextConfig",
            factory
        ]);
})(angular);

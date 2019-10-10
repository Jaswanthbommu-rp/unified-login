//  Reset Password Form Config

(function(angular, undefined) {
    "use strict";

    function factory($filter, baseFormConfig, inputTextConfig) {
        var model = baseFormConfig(),
            lang = $filter("resetPasswordText");

        model.oldPassword = inputTextConfig({
            required: true,
            fieldName: "oldPassword",
            dataType: "password",
            disabled: false,
            modelOptions: {
                allowInvalid: true,
                updateOn: "default"
            },

            trimInput: false,

            errorMsgs: [{
                name: "required",
                text: lang("errorMsgs.currentPassword.required")
            }]
        });

        model.newPassword = inputTextConfig({
            required: true,
            fieldName: "newPassword",
            dataType: "password",
            disabled: false,
            modelOptions: {
                allowInvalid: true,
                updateOn: "default"
            },
            autocomplete: "new-password",
            trimInput: false,

            asyncValidators: {
                isValid: model.getMethod("passwordIsValid"),
                isSameAsCurrent: model.getMethod("newPasswordMatchesCurrent")
            },

            errorMsgs: [{
                name: "required",
                text: lang("errorMsgs.password.required")
            }, {
                name: "isValid",
                text: lang("errorMsgs.password.incompleteRequirements")
            }, {
                name: "isSameAsCurrent",
                text: lang("errorMsgs.password.notSameAsCurrent")
            }]
        });

        model.newPasswordCopy = inputTextConfig({
            required: true,
            fieldName: "newPasswordCopy",
            dataType: "password",
            disabled: false,
            modelOptions: {
                allowInvalid: true,
                updateOn: "default"
            },

            autocomplete: "new-password",
            trimInput: false,

            asyncValidators: {
                isMatch: model.getMethod("passwordCopyIsValid")
            },

            errorMsgs: [{
                name: "required",
                text: lang("errorMsgs.confirmPassword.required")
            }, {
                name: "isMatch",
                text: lang("errorMsgs.confirmPassword.mismatch")
            }]
        });

        model.setControlsDisabledState = function (bool) {
            model.oldPassword.disabled = bool;
            model.newPassword.disabled = bool;
            model.newPasswordCopy.disabled = bool;
        };

        return model;
    }

    angular
        .module("settings")
        .factory("ResetPasswordFormConfig", [
            "$filter",
            "baseFormConfig",
            "rpFormInputTextConfig",
            factory
        ]);
})(angular);

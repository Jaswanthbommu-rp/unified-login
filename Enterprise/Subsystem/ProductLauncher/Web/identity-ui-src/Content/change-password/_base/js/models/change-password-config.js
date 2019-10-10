//  Change Password Form Config Model

(function (angular) {
    "use strict";

    function factory($filter, baseFormConfig, inputConfig) {
		var model = baseFormConfig();

		model.createPassword = inputConfig({
            id: "createPassword",
            fieldName: "createPassword",
            trimInput: false,
		    maxlength: 100,
            dataType: "password",
            placeholder: $filter("changePasswordText")("label.placeholder.newPassword"),
            errorMsgs: [
                {
                    name: "passChecklist",
                    text: $filter("changePasswordText")("errorMsgs.password.incompleteRequirements")
                },
                {
                    name: "noUsername",
                    text: $filter("changePasswordText")("errorMsgs.password.noSameUsername")
                },
                {
                    name: "limitedHistory",
                    text: $filter("changePasswordText")("errorMsgs.password.historyRecord")
                }
            ],
            asyncValidators: {
                limitedHistory: model.getMethod("limitedHistory")
            },
            validators: {
                passChecklist: model.getMethod("passChecklist"),
                confirmNewPassword: model.getMethod("confirmNewPassword"),
                noUsername: model.getMethod("noUsername")
            },
            modelOptions: {
                allowInvalid: true
            },
            onChange: model.getMethod("confirmNewPassword")
        });

		model.confirmPassword = inputConfig({
            id: "confirmPassword",
            fieldName: "confirmPassword",
            trimInput: false,
            maxlength: 100,
            required: true,
            dataType: "password",
            placeholder: $filter("changePasswordText")("label.placeholder.confirmPassword"),
            errorMsgs: [
                {
                    name: "required",
                    text: $filter("changePasswordText")("errorMsgs.confirmPassword.required")
                },
                {
                    name: "confirmValidPassword",
                    text: $filter("changePasswordText")("errorMsgs.confirmPassword.mismatch")
                }
            ],
            validators: {
                confirmValidPassword: model.getMethod("confirmValidPassword")
            },
            modelOptions: {
                allowInvalid: true
            },
            onChange: model.getMethod("confirmValidPassword")
        });

	    return model;
    }

    angular
        .module("identity")
        .factory("userPasswordConfig", [
                "$filter",
        		"baseFormConfig",
        		"rpFormInputTextConfig",
        		factory
    		]);
})(angular);
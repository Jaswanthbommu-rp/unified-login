//  Change Password Form Config Model

(function (angular) {
    "use strict";

    function factory($filter, baseFormConfig, inputConfig) {
		var model = baseFormConfig();

		model.createPassword = inputConfig({
		    id: "createPassword",
		    fieldName: "createPassword",
            required: true,
		    maxlength: 100,
            trimInput: false,
            dataType: "password",
            autocomplete: "new-password",
            placeholder: $filter("setPasswordText")("label.placeholder.newPassword"),
            errorMsgs: [
                {
                    name: "passChecklist",
                    text: $filter("setPasswordText")("errorMsgs.password.incompleteRequirements")
                },
                {
		            name: "noUsername",
		            text: $filter("setPasswordText")("errorMsgs.password.noSameUsername")
                },
                {
                    name: "minRequirements",
                    text: $filter("setPasswordText")("errorMsgs.password.historyRecord")
                }
            ],
            asyncValidators: {
                minRequirements: model.getMethod("validatePassword")
            },
            validators: {
                passChecklist: model.getMethod("validateChecklist"),
                noUsername: model.getMethod("validateNoUsername")
            },
            modelOptions: {
                allowInvalid: true
            },
            onChange: model.getMethod("validateConfirmPassword")

        });

		model.confirmPassword = inputConfig({
		    id: "confirmPassword",
		    fieldName: "confirmPassword",
            required: true,
            maxlength: 100,
            trimInput: false,            
            dataType: "password",
            autocomplete: "new-password",
            placeholder: $filter("setPasswordText")("label.placeholder.confirmPassword"),
            errorMsgs: [
                {
                    name: "required",
                    text: $filter("setPasswordText")("errorMsgs.confirmPassword.required")
                },
                {
                    name: "confirmValidPassword",
                    text: $filter("setPasswordText")("errorMsgs.confirmPassword.mismatch")
                }
            ],
            validators: {
                confirmValidPassword: model.getMethod("confirmValidPassword")
            },
            modelOptions: {
                allowInvalid: true
            }
        });

        model.setErrorMessage = function(fieldName, errorType, errorMessage) {
            if(model[fieldName]) {
                angular.forEach(model[fieldName].errorMsgs, function(val, key) {
                    if(val.name == errorType) {
                        val.text = errorMessage;
                    }
                });
            }
        };

	    return model;
    }

    angular
        .module("new-user")
        .factory("userPasswordConfig", [
                "$filter",
        		"baseFormConfig",
        		"rpFormInputTextConfig",
        		factory
    		]);
})(angular);
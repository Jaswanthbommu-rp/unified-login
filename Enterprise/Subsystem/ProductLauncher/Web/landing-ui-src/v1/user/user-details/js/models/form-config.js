//  User Details Form Config Model

(function (angular, undefined) {
    "use strict";

    function factory($filter, regex, baseFormConfig, textConfig, menuConfig, switchConfig, datepickerConfig, filterType) {
        var loginNameErrorReqd,
            notificationEmailReqd,
            notificationEmailReqdProducts = [],
            model = baseFormConfig(),
            lang = $filter("userDetailsText");

        loginNameErrorReqd = {
            name: "required",
            text: lang("errorMsgs.loginName.required")
        };

        notificationEmailReqd = {
            name: "required",
            text: lang("errorMsgs.notificationEmail.required")
        };

        model.firstName = textConfig({
            fieldName: "firstName",
            required: true,
            disabled: false,
            errorMsgs: [
                {
                    name: "required",
                    text: lang("errorMsgs.firstName.required")
                }
            ],
            maxlength: 50
        });

        model.middleName = textConfig({
            fieldName: "middleName",
            disabled: false,
            maxlength: 50
        });

        model.lastName = textConfig({
            fieldName: "lastName",
            required: true,
            disabled: false,
            errorMsgs: [
                {
                    name: "required",
                    text: lang("errorMsgs.lastName.required")
                }
            ],
            maxlength: 50
        });

        model.userTypeId = menuConfig({
            nameKey: "name",
            valueKey: "partyRoleTypeId",
            fieldName: "userType",
            required: true,
            onChange: model.getMethod("onUserTypeChange"),
            errorMsgs: [
                {
                    name: "required",
                    text: lang("errorMsgs.userTypeId.required")
                }
            ]
        });

        model.loginName = textConfig({
            id: "loginName",
            fieldName: "loginName",
            required: true,
            disabled: false,
            autocomplete: "new-password",
            asyncValidators: {
                validLoginName: model.getMethod("validateLoginName")
            },
            modelOptions: {
                allowInvalid: true
            },
            errorMsgs: [
                loginNameErrorReqd,
                {
                    name: "validLoginName",
                    text: lang("errorMsgs.loginName.invalidLoginName")
                }
            ],
            maxlength: 255,
            onBlur: model.getMethod("openExternalUserModal"),
            
        });

        model.password = textConfig({
            dataType: "password",
            fieldName: "password",
            autocomplete: "new-password",
            required: true,
            validators: {
                validPassword: model.getMethod("validatePassword")
            },
            modelOptions: {
                allowInvalid: true
            },
            errorMsgs: [
                {
                    name: "required",
                    text: lang("errorMsgs.password.required")
                },
                {
                    name: "validPassword",
                    text: lang("errorMsgs.password.validPassword")
                }
            ],
            trimInput: false,
            onChange: model.getMethod("validatePasswordCopy"),
            onBlur: model.getMethod("hidePasswordRequirements"),
            onFocus: model.getMethod("showPasswordRequirements")
        });

        model.passwordCopy = textConfig({
            dataType: "password",
            fieldName: "passwordCopy",
            autocomplete: "new-password",
            required: true,
            modelOptions: {
                allowInvalid: true
            },
            errorMsgs: [
                {
                    name: "required",
                    text: lang("errorMsgs.passwordCopy.required")
                },
                {
                    name: "passwordsMatch",
                    text: lang("errorMsgs.passwordCopy.passwordsMatch")
                }
            ],
            asyncValidators: {
                passwordsMatch: model.getMethod("checkPasswordMatch")
            },
            trimInput: false
        });

        model.notificationEmail = textConfig({
            fieldName: "notificationEmail",
            disabled: false,
            pattern: regex.email,
            errorMsgs: [
                notificationEmailReqd,
                {
                    name: "pattern",
                    text: lang("errorMsgs.notificationEmail.pattern")
                }
            ]
        });

        model.fromDate = datepickerConfig({
            required: true,
            format: "MM/DD/YYYY",
            fieldName: "fromDate",
            disabled: false,
            onChange: model.getMethod("updateExpiresLimit"),
            errorMsgs: {
                required: lang("errorMsgs.fromDate.required")
            }
        });

        model.thruDate = datepickerConfig({
            format: "MM/DD/YYYY",
            disabled: false,
            fieldName: "thruDate"
        });

        model.contactMethod = menuConfig({
            required: false,
            nameKey: "name",
            valueKey: "preferredContactMethodId",
            fieldName: "contact-method",
            // onChange: model.getMethod("validatePhoneNumber"),
            errorMsgs: [
                {
                    name: "required",
                    text: "Preferred contact method is required"
                }
            ]
        });

        model.jobTitle = menuConfig({
            nameKey: "name",
            valueKey: "partyRoleTypeId",
            fieldName: "job-title",
            required: false,
            errorMsgs: [
                {
                    name: "required",
                    text: "Job title is required"
                }
            ]
        });

        model.title = textConfig({
            maxlength: 50,
            id: "companyJobTitle",
            fieldName: "companyJobTitle"
        });

        model.phoneNumber = textConfig({
            id: "phoneNumber",
            fieldName: "phoneNumber",
            required: false,
            minlength: 10,
            maxlength: 10,
            pattern: /^[\d]+$/,
            inputFilter: filterType.numeric,
            errorMsgs: [
                {
                    name: "isRequired",
                    text: "Phone number is required"
                    },
                {
                    name: "minlength",
                    text: "A ten digit phone number is required"
                    },
                {
                    name: "pattern",
                    text: "Phone number should only contain digits"
                    }
            ]
        });

        model.phoneType = menuConfig({
            required: false,
            nameKey: "name",
            valueKey: "contactMechanismUsageTypeId",
            errorMsgs: [{
                name: "required",
                text: "Phone type is required"
            }],

        });

        model.setThruDateMinLimit = function (minDate) {
            model.thruDate.minDate(minDate);
        };

        model.setFromDateMinLimit = function (minDate) {
            model.fromDate.minDate(minDate);
        };

        model.setLoginNameErrorReqdText = function (id) {
            loginNameErrorReqd.text = lang("errorMsgs.loginName.required", id);
        };

        model.setNotificationEmailRequired = function (val, data) {
            if (val) {
                if (notificationEmailReqdProducts.indexOf(data) == -1) {
                    notificationEmailReqdProducts.push(data);
                }
            }
            else {
                if (notificationEmailReqdProducts.indexOf(data) != -1) {
                    notificationEmailReqdProducts.splice(notificationEmailReqdProducts.indexOf(data), 1);
                }
            }
            model.notificationEmail.required = (notificationEmailReqdProducts.length > 0);

            notificationEmailReqd.text = "";
            if (model.notificationEmail.required) {
                notificationEmailReqd.text = lang("errorMsgs.notificationEmail.required", model.setNotificationEmailMessage(notificationEmailReqdProducts));
            }
        };

        model.setNotificationEmailMessage = function (notificationEmailReqdProducts) {
            var totalProducts = notificationEmailReqdProducts.length;
            var messageInsertText = notificationEmailReqdProducts[0];
            if (totalProducts > 1) {
                for (var i = 1; i < (totalProducts - 1); i++) {
                    messageInsertText += ", " + notificationEmailReqdProducts[i];
                }
                messageInsertText += " and " + notificationEmailReqdProducts[totalProducts - 1];
                messageInsertText += " are ";
            }
            else {
                messageInsertText += " is ";
            }
            return messageInsertText;
        };

        model.clearNotificationEmail = function () {
            model.notificationEmail.required = false;
            notificationEmailReqd.text = "";
            notificationEmailReqdProducts = [];
        };

        model.setUserTypeOptions = function (options) {
            options.forEach(function (option) {
                var id = option.partyRoleTypeId;
                option.name = lang("options.userTypeId." + id);
            });
            options = $filter("orderBy")(options, "-name");

            model.userTypeId.setOptions(options);
        };

        // model.setUserTimeZoneOptions = function (options) {
        //     model.TimeZoneOffset.setOptions(options);
        // };

        model.setContactMethodOptions = function (options) {
            model.contactMethod.setOptions(options);
        };

        model.setPhoneTypeOptions = function (options) {
            model.phoneType.setOptions(options);
        };

        model.setJobTitleOptions = function (options) {
            model.jobTitle.setOptions(options);
        };

        model.setEmailRequired = function (required) {
            model.password.required = required;
            model.passwordCopy.required = required;
        };

        model.setFirstNameDisabled = function (bool) {
            model.firstName.disabled = bool;
        };

        model.setMiddleNameDisabled = function (bool) {
            model.middleName.disabled = bool;
        };

        model.setLastNameDisabled = function (bool) {
            model.lastName.disabled = bool;
        };

        model.setLoginNameDisabled = function (bool) {
            model.loginName.disabled = bool;
        };

        model.setControlsDisabledState = function (bool) {
            model.firstName.disabled = bool;
            model.middleName.disabled = bool;
            model.lastName.disabled = bool;
            model.loginName.disabled = bool;
            model.notificationEmail.disabled = bool;
            model.fromDate.disabled = bool;
            model.thruDate.disabled = bool;
            // model.TimeZoneOffset.setData({
            //     readonly: bool
            // });
        };

        return model;
    }

    angular
        .module("settings")
        .factory("userDetailsFormConfig", [
            "$filter",
            "regex",
            "baseFormConfig",
            "rpFormInputTextConfig",
            "rpFormSelectMenuConfig",
            "rpSwitchConfig",
            "rpDatetimepickerConfig",
             "rpInputFilterType",
            factory
        ]);
})(angular);

//  Add User Form Config Model

(function (angular) {
    "use strict";

    function factory($filter, baseFormConfig, inputTextConfig, addPersonaBtnConfig, enableSwitchConfig, 
            enterpriseRoleConfig, notificationEmailConfig, userDatePickerConfig, usernameConfig, userTypeConfig, moment) {
        
        var model = baseFormConfig();

        model.firstName = inputTextConfig({
            id: "firstname",
            fieldName: "firstname",
            required: true,
            errorMsgs: [{
                "name": "required",
                "text": $filter("userDetailsText")("err_first_name_required")
            }],
            label: $filter("userDetailsText")("user_detail_first_name"),
            maxlength: 100
        });

        model.lastName = inputTextConfig({
            id: "lastname",
            fieldName: "lastname",
            required: true,
            errorMsgs: [{
                "name": "required",
                "text": $filter("userDetailsText")("err_last_name_required")
            }],
            label: $filter("userDetailsText")("user_detail_last_name"),
            maxlength: 100
        });

        model.middleInitial = inputTextConfig({
            id: "middleInitial",
            fieldName: "middleInitial",
            label: $filter("userDetailsText")("user_detail_middle_init")            
        });

        model.password = inputTextConfig({
            id: "password",
            fieldName: "password",
            dataType: "password",
            required: false,
            maxLength: 24,
            modelOptions: {
                allowInvalid: true,
            },
            validators: {
                passRequirements: model.getMethod("validatePassword"),
                passNoSameUserandPass: model.getMethod("checkUsernamePassword")
            },
            errorMsgs: [{
                name: "passRequirements",
                text: $filter("userDetailsText")("err_pass_requirements")   
            },
            {
                "name": "required",
                "text": $filter("userDetailsText")("err_password_required")
            },
            {
                "name": "passNoSameUserandPass",
                "text": $filter("userDetailsText")("err_password_sameusername")
            }],
            label: $filter("userDetailsText")("user_detail_password")
        });

        model.confirmPassword = inputTextConfig({
            id: "confirmPassword",
            fieldName: "confirmPassword",
            dataType: "password",
            required: false,
            maxlength: 24,
            validators: {
                matchPassword: model.getMethod("matchPassword")
            },
            errorMsgs: [{
                name: "matchPassword",
                text: $filter("userDetailsText")("err_pass_no_match")
            },
            {
                "name": "required",
                "text": $filter("userDetailsText")("err_password_required")
            }],
            label: $filter("userDetailsText")("user_detail_confirm_password")
        });

        model.addPersona = addPersonaBtnConfig.get();
        model.enableUser = enableSwitchConfig.get();
        model.enterpriseRole = enterpriseRoleConfig.get();
        model.notificationEmail = notificationEmailConfig.get();
        model.username = usernameConfig.get({
            maxlength: 255
        });

        model.userType = userTypeConfig.get({
            onChange: model.methods.get("updateBasedOnUserType")
        });

        model.startDate = userDatePickerConfig.get("start", {
            onChange: model.methods.get("onStartDateChange"),
            label: $filter("userDetailsText")("user_detail_effective_date")
        });

        model.endDate = userDatePickerConfig.get("end", {
            onChange: model.methods.get("onEndDateChange"),
            label: $filter("userDetailsText")("user_detail_expiration_date")
        });

        model.updatePasswordRequired = function(fieldName, required) {
            if(model[fieldName]) {
                model[fieldName].required = required;
            }
        };

        model.changeLabel = function(fieldName, label, errorText) {
            if (model[fieldName]) {
                model[fieldName].label = label;
                model[fieldName].errorMsgs[0].text = errorText;
            } else {
                logc("userDetailsFormConfig.label: " + fieldName + " is not a valid field name!");
            }

            return model;
        };

        model.setVisibility = function(fieldName, flag) {
            if(model[fieldName]) {
                model[fieldName].isVisible = flag;
            } else {
                logc("userDetailsFormConfig.isVisible: " + fieldName + " is not a valid field name!");
            }

            return model;
        };

        model.setAccess = function(fieldName, flag) {
            if(model[fieldName]) {
                model[fieldName].disabled = flag;
            } else {
                logc("userDetailsFormConfig.disabled: " + fieldName + " is not a valid field name!");                   
            }
        };

        return model;
    }

    angular
        .module("settings")
        .factory("userDetailsFormConfig", [
            "$filter",
            "baseFormConfig",
            "rpFormInputTextConfig",
            "addPersonaBtnConfig",
            "enableSwitchConfig",
            "enterpriseRoleConfig",
            "notificationEmailConfig",
            "userDatePickerConfig",
            "usernameConfig",
            "userTypeMenuConfig",
            "moment",
            factory
        ]);
})(angular);

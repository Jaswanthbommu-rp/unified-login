//  Add User Form Config Model

(function (angular) {
    "use strict";

    function factory($filter, baseFormConfig, inputTextConfig, dateTimePickerConfig, userTypeConfig) {
        var model = baseFormConfig();

        model.firstName = inputTextConfig({
            id: "firstname",
            fieldName: "firstname",
            required: true,
            errorMsgs: [{
                "name": "required",
                "text": $filter("userDetailsText")("err_first_name_required")
            }]
        });

        model.lastName = inputTextConfig({
            id: "lastname",
            fieldName: "lastname",
            required: true,
            errorMsgs: [{
                "name": "required",
                "text": $filter("userDetailsText")("err_last_name_required")
            }]
        });

        model.middleInitial = inputTextConfig({
            id: "middleInitial",
            fieldName: "middleInitial",
        });

        model.username = inputTextConfig({
            id: "username",
            fieldName: "username",
            required: true,
            errorMsgs: [{
                "name": "required",
                "text": $filter("userDetailsText")("err_username_required")
            }], 
            label: $filter("userDetailsText")("username_email")
        });

        model.email = inputTextConfig({
            id: "email",
            fieldName: "email"
        });

        model.password = inputTextConfig({
            id: "password",
            fieldName: "password",
            dataType: "password",
            asyncVaidators: {
                passRequirements: model.getMethod("validatePassword")
            },
            errorMsgs: [{
                name: "passRequirements",
                text: "Password does not meet requirements"
            }]
        });

        model.enterpriseRole = inputTextConfig({
            id: "enterprise-role",
            fieldName: "enterpriseRole",
            iconClass: "rp-icon-search2"
        });

        model.startDate = dateTimePickerConfig({
            id: "startDate",
            fieldName: "startDate",
            required: false,
            iconClass: "rp-icon-calendar",
            format: "MM/DD/YYYY",
            onChange: model.methods.get("onStartDateChange")
        });

        model.endDate = dateTimePickerConfig({
            id: "endDate",
            fieldName: "endDate",
            required: false,
            iconClass: "rp-icon-calendar",
            format: "MM/DD/YYYY",
            onChange: model.methods.get("onEndDateChange")
        });

        model.enableUser = {
            id: "enableUser"
        };

        model.userType = userTypeConfig.get({
            onChange: model.getMethod("updateBasedOnUserType")
        });

        model.changeLabel = function(fieldName, label) {
            if (model[fieldName]) {
                model[fieldName].label = label;
            }
            else {
                logc("userDetailsFormConfig.label: " + fieldName + " is not a valid field name!");
            }

            return model;
        };

        return model;
    }

    angular
        .module("settings")
        .factory("userDetailsFormConfig", [
            "$filter",
            "baseFormConfig",
            "rpFormInputTextConfig",
            "rpDatetimepickerConfig",
            "userTypeMenuConfig",
            factory
        ]);
})(angular);

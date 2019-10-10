//  Start Profile Form Config Model

(function (angular) {
    "use strict";

    function factory($filter, baseFormConfig, inputTextConfig, menuConfig, startProfileFormModel, filterType, regex) {
        var model = baseFormConfig();

        model.username = inputTextConfig({
        	id: "username",
        	fieldName: "username",
        	required: true,
            disabled: true,
            errorMsgs: [{
                name: "required",
                text: $filter("startProfileText")("profile_req_err") 
            }]
        });

        model.industryJobTitle = menuConfig({
            required: true,
            nameKey: "name",
            valueKey: "partyRoleTypeId",
            errorMsgs: [{
                name: "required",
                text: $filter("startProfileText")("profile_req_err") 
            }]
        });

    	model.companyJobTitle = inputTextConfig({
            id: "companyJobTitle",
            fieldName: "companyJobTitle",
            required: true,
            errorMsgs: [{
                name: "required",
                text: $filter("startProfileText")("profile_req_err") 
            }]
        });

        model.phoneNumber = inputTextConfig({
            id: "phoneNumber",
            fieldName: "phoneNumber",
            required: true,
            minlength: 10,
            maxlength: 10,
            pattern: /^[\d]+$/,
            inputFilter: filterType.numeric,
            errorMsgs: [{
                name: "required",
                text: $filter("startProfileText")("profile_req_err") 
            }, {
                name: "minlength",
                text: "A ten digit phone number is required" //TODO JSLANG
            }, {
                name: "pattern",
                text: "Phone number should only contain digits" //TODO JSLANG
            }],
            onChange: model.getMethod("onChangePh"),
        });

        //TODO JSLANG
        model.secondaryEmail = inputTextConfig({
            required: false,
            // dataType: "email",
            pattern: regex.email,
            errorMsgs: [
                {
                    name: "email",
                    text: "Email should be a valid email address"
                }
            ]
        });

        model.phoneType = menuConfig({
            required: true,
            nameKey: "name",
            valueKey: "contactMechanismUsageTypeId",
            errorMsgs: [{
                name: "required",
                text: $filter("startProfileText")("profile_req_err") 
            }],
            validators: {
                required: model.getMethod("checkPhoneValid")
            },
        });

        model.setOptions = function (fieldName, fieldOptions) {
            if (model[fieldName]) {
                model[fieldName].setOptions(fieldOptions);
            }
            else {
                logc("sampleSelectMenuFormConfig.setOptions: " + fieldName + " is not a valid field name!");
            }

            return model;
        };

        return model;
    }

    angular
        .module("new-user")
        .factory("startProfileFormConfig", [
            "$filter",
            "baseFormConfig",
        	"rpFormInputTextConfig",
            "rpFormSelectMenuConfig",
        	"startProfileFormModel",
            "rpInputFilterType",
            "regex",
        	factory
        ]);
})(angular);

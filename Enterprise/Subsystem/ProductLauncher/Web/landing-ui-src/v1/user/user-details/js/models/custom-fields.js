//  User Custom Fields Model

(function (angular, undefined) {
    "use strict";

    function factory(inputConfig, filterType) {
        var index = 0;

        function UserCustomFieldsModel() {
            var s = this;
            index++;
            s.init();
        }

        var p = UserCustomFieldsModel.prototype;

        p.init = function () {
            var s = this;

            s.index = index;
            s.fieldConfig = "";
            s.data = {
                id: 0,
                sequence: 0,
                fieldName: "",
                fieldValue: "",
                fieldType: "",
                minCharlength: 0,
                maxCharlength: 0,
                isRequiredField: true,
                isFieldEnabled: true
            };

            s.customFieldConfig = {
                required: "",
                minlength: "",
                maxlength: "",
                disabled: "",
                id: "",
				userLoginPersonaId: "",
                fieldValueId: "",
                fieldName: "",
                dataType: "",
                inputFilter: "",
                pattern: "",
                modelOptions: {
                    allowInvalid: true
                },
                errorMsgs: []
            };
        };

        p.changeCallback = function () {
            var s = this;
            s.onChange.publish();
            return s;
        };

        p.getData = function () {
            var s = this;
            return {
                fieldValueId: s.data.fieldValueId,
				userLoginPersonaId: s.data.userLoginPersonaId,
                fieldId: s.data.fieldId,
                sequence: s.data.sequence,
                name: s.data.name,
                value: s.data.value,
                fieldTypeName: s.data.fieldTypeName,
                minCharLength: s.data.minCharLength,
                maxCharLength: s.data.maxCharLength,
                required: s.data.required,
                enabled: s.data.enabled
            };
        };

        p.getId = function () {
            var s = this;
            return s.index;
        };

        p.getCustomFieldId = function () {
            var s = this;
            return s.data.id;
        };

        p.getRawData = function () {
            var s = this;
            return s.data;
        };

        p.requireCustomField = function () {
            var s = this;
            return s.data.isRequiredField;
        };

        p.setData = function (data) {
            var s = this,
                pattern = "",
                minLengthText = "",
                patternText = "",
                requiredText = "";

            s.data = data || {};

            if (s.data.fieldValueId === 0) {
                s.data.fieldValueId = "";
            }

            if (!s.data.userLoginPersonaId) {
                s.data.userLoginPersonaId = 0;
            }

            requiredText = data.name + " is required";
            minLengthText = data.name + " should have at least " + data.minCharLength + " characters";

            if (data.fieldTypeName.toLowerCase() === "numeric") {
                pattern = /^[0-9]+$/;
                patternText = data.name + " should only contain numbers";
                s.customFieldConfig.inputFilter = filterType.numeric;
            }

            var rqError = {
                name: "required",
                text: requiredText
            };
            var minError = {
                name: "minlength",
                text: minLengthText
            };
            var patError = {
                name: "pattern",
                text: patternText
            };

            s.customFieldConfig.errorMsgs.push(rqError);
            s.customFieldConfig.errorMsgs.push(minError);
            s.customFieldConfig.errorMsgs.push(patError);

            s.customFieldConfig.required = data.required;
            s.customFieldConfig.minlength = data.minCharLength;
            s.customFieldConfig.maxlength = data.maxCharLength;
            s.customFieldConfig.disabled = !data.enabled;
            s.customFieldConfig.id = data.fieldId;
            s.customFieldConfig.fieldValueId = data.fieldValueId;
			s.customFieldConfig.userLoginPersonaId = data.userLoginPersonaId;
            s.customFieldConfig.fieldName = data.name;
            s.customFieldConfig.dataType = "Alphanumeric";
            s.customFieldConfig.pattern = pattern;
            s.fieldConfig = inputConfig(s.customFieldConfig);

            return s;
        };

		p.destroy = function () {
            var s = this;
            s.onChange.destroy();
            s.customFieldConfig.destroy();
            s.customFieldConfig = undefined;
        };

        return function (data) {
            return (new UserCustomFieldsModel()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("userCustomFieldsModel", [
            "rpFormInputTextConfig",
            "rpInputFilterType",
            factory
            ]);
})(angular);

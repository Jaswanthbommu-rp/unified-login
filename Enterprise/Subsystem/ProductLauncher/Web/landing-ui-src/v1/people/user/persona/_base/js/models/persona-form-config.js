//  Persona Form Config Model

(function (angular) {
    "use strict";

    function factory(baseFormConfig, inputTextConfig, dateTimePickerConfig, personaTypeConfig) {
        var model = baseFormConfig();

        model.personaName = inputTextConfig({
        	id: "personaName",
        	fieldName: "personaName",
        	required: true,
            maxlength: 30,
            onChange: model.methods.get("onPersonaNameChange"),
        });

        model.startDate = dateTimePickerConfig({
            id: "startDate",
            fieldName: "startDate",
            iconClass: "rp-icon-calendar",
            format: "MM/DD/YYYY",
            onChange: model.methods.get("onStartDateChange")
        });

        model.endDate = dateTimePickerConfig({
            id: "endDate",
            fieldName: "endDate",
            iconClass: "rp-icon-calendar",
            format: "MM/DD/YYYY",
            onChange: model.methods.get("onEndDateChange")
        });

        model.personaType = personaTypeConfig.get();

        return model;
    }

    angular
        .module("settings")
        .factory("managePersonaFormConfig", [
            "baseFormConfig",
        	"rpFormInputTextConfig",
        	"rpDatetimepickerConfig",
            "personaTypeMenuConfig",
        	factory
        ]);
})(angular);

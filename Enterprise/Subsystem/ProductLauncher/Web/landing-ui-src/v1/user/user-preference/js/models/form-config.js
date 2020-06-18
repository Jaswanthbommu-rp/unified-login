(function (angular) {
    "use strict";

    function factory(
        baseFormConfig,
        menuConfig
    ) {

        var model = baseFormConfig();

        model.preferenceConfig = menuConfig({
            id: 'preference',
            fieldName: 'preference',
            nameKey: "text",
            valueKey: "value",
        });


        model.setOptions = function(fieldName, fieldOptions) {
            if (model[fieldName]) {
                model[fieldName].setOptions(fieldOptions);
            } else {
                logc('rpFormSelectMenuConfig.setOptions: ' + fieldName + ' is not a valid field name!');
            }
        };

        return model;
    }
    angular
        .module("settings")
        .factory("UserPreferenceFormConfig", [
            "baseFormConfig",
            "rpFormSelectMenuConfig",
            factory
        ]);
})(angular);

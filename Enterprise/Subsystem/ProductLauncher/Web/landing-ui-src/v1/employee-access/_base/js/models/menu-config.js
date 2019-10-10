// form menu config
(function(angular) {
    "use strict";

    function factory(baseFormConfig, menuConfig) {
        var model = baseFormConfig();

        model.optionsData = menuConfig({
            nameKey: "label",
            valueKey: "value",
            onChange: model.getMethod("menuChange")
        });

        model.setOptions = function(fieldName, fieldOptions) {

            if (model[fieldName]) {
                model[fieldName].setOptions(fieldOptions);
            }
            return model;
        };

        model.setOptionsFilter = function(fieldName, filter) {
            model[fieldName].setOptionsFilter(filter);
            return model;
        };

        return model;
    }

    angular
        .module("settings")
        .factory("empAccessMenuConfig", [
            "baseFormConfig",
            "rpFormSelectMenuConfig",
            factory
        ]);
})(angular);
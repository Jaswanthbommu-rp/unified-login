// form menu config
(function(angular) {
    "use strict";

    function factory(baseFormConfig, menuConfig) {
        var model = baseFormConfig();

        model.optionsData = menuConfig({
            nameKey: "productName",
            valueKey: "productVal",
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
        .factory("userMgmtProductsConfig", [
            "baseFormConfig",
            "rpFormSelectMenuConfig",
            factory
        ]);
})(angular);
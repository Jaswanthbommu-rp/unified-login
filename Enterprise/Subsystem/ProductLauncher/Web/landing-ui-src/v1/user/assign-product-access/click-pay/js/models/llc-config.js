//  Llc Form Config

(function(angular, undefined) {
    "use strict";

    function factory(baseFormConfig, inputTextConfig, inputChkConfig) {
        var model = baseFormConfig();
        
        model.searchRightName = inputTextConfig({
            required: false,
            placeholder : "Search LLC",
            onChange: model.getMethod("searchRight"),
        });

        model.selectAll = inputChkConfig({
            trueValue: true,
            disabled: false,
            falseValue: false,
            onChange: model.getMethod("onSelAllChange"),
            
        });
               

        return model;
    }

    angular
        .module("settings")
        .factory("cpFormConfig", [
            "baseFormConfig",
            "rpFormInputTextConfig",
            "rpFormInputCheckboxConfig",
            factory
        ]);
})(angular);
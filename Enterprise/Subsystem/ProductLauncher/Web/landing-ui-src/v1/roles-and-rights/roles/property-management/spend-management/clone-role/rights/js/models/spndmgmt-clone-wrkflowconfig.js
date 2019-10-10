//  Assign Role Form Config

(function(angular, undefined) {
    "use strict";

    function factory(baseFormConfig, inputTextConfig) {
        var model = baseFormConfig();

        model.orderTimeOut = inputTextConfig({
            required: true,
            maxlength : 4,
            pattern: /^[0-9]+$/i,
            suffix: "hrs",
            errorMsgs: [{
                name: "pattern",
                text: "Invalid input"
            }]
            
        });

        model.invoiceTimeOut = inputTextConfig({
            required: true,
            maxlength : 4,
            pattern: /^[0-9]+$/i,
            suffix: "hrs",
            errorMsgs: [{
                name: "pattern",
                text: "Invalid input"
            }]
        });

        return model;
    }

    angular
        .module("settings")
        .factory("spndmgmtCloneRoleWrkFloFormConfig", [
            "baseFormConfig",
            "rpFormInputTextConfig",
            factory
        ]);
})(angular);
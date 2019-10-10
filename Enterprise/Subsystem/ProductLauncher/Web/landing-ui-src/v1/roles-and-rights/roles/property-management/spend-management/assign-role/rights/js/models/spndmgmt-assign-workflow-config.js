//  Assign Role Form Config

(function(angular, undefined) {
    "use strict";

    function factory(baseFormConfig, inputTextConfig, security) {
        var model = baseFormConfig();

        model.orderTimeOut = inputTextConfig({
            required: true,
            readonly: security.isAllowed("viewRoleRight") === true? true : false,
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
            readonly: security.isAllowed("viewRoleRight") === true? true : false,
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
        .factory("spndmgmtAssignRoleWrkFloFormConfig", [
            "baseFormConfig",
            "rpFormInputTextConfig",
             "routeSecurity",
            factory
        ]);
})(angular);
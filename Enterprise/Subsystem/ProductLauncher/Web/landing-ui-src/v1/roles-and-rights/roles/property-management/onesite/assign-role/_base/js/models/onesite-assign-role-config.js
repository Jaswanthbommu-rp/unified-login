//  New Role Form Config

(function(angular, undefined) {
    "use strict";

    function factory(baseFormConfig, inputTextConfig) {
        var model = baseFormConfig();

        model.assignRoleName = inputTextConfig({
            required: true,
            maxlength : 70,
            readonly : false,
            errorMsgs: [{
                name: "required",
                text: "Role name is required"                
            }]
        });

        return model;
    }

    angular
        .module("settings")
        .factory("onesiteAssignRoleFormConfig", [
            "baseFormConfig",
            "rpFormInputTextConfig",
            factory
        ]);
})(angular);
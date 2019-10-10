//  New Role Form Config

(function (angular, undefined) {
    "use strict";

    function factory(baseFormConfig,  inputTextConfig) {
        var model = baseFormConfig();

        model.roleName = inputTextConfig({
            required: true,
            maxlength : 70,
            errorMsgs: [
                {
                    name: "required",
                    text: "Role name is required"
                }
            ]
        });        

        return model;
    }

    angular
        .module("settings")
        .factory("userMgmtNewRoleFormConfig", [
            "baseFormConfig",            
            "rpFormInputTextConfig",            
            factory
        ]);
})(angular);

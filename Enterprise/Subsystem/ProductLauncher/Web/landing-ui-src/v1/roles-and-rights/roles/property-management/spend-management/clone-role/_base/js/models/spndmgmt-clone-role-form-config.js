//  Clone Role Form Config

(function(angular, undefined) {
    "use strict";

    function factory(baseFormConfig, inputTextConfig) {
        var model = baseFormConfig();

        model.roleName = inputTextConfig({
            required: true,
            maxlength : 70,
            errorMsgs: [{
                name: "required",
                text: "Role name is required"                
            }]
        });

        model.roleDesc = inputTextConfig({
            required: false,
            maxlength : 100,
            errorMsgs: [{
                // name: "required",
                // text: "Role Desc is required"                
            }]
        });

        model.searchRightName = inputTextConfig({
            required: false,
            placeholder : "Right Name",
            onChange: model.getMethod("searchRight"),
        });

        return model;
    }

    angular
        .module("settings")
        .factory("spndmgmtCloneRoleFormConfig", [
            "baseFormConfig",
            "rpFormInputTextConfig",
            factory
        ]);
})(angular);
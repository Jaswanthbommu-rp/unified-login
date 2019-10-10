//  New Role Form Config

(function(angular, undefined) {
    "use strict";

    function factory(baseFormConfig, inputTextConfig) {
        var model = baseFormConfig();

        model.assignRoleName = inputTextConfig({
            required: true,
            maxlength : 40,
            readonly : false,
            errorMsgs: [{
                name: "required",
                text: "Role name is required"                
            }]
        });

        model.assignRoleDesc = inputTextConfig({
            required: false,
            maxlength : 100,
            readonly : false,
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
        .factory("spndmgmtAssignRoleFormConfig", [
            "baseFormConfig",
            "rpFormInputTextConfig",
            factory
        ]);
})(angular);
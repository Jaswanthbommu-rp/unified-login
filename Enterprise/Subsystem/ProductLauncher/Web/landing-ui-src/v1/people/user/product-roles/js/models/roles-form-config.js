// Product Item Roles Access Form Configuration

(function (angular) {
    "use strict";

    function factory(baseFormConfig, inputTextConfig) {
        var model = baseFormConfig();

        model.searchText = inputTextConfig({
            fieldName: "rolesSearchText",
            iconClass: "rp-icon-search2",
            onChange: model.getMethod("filterProperties")
        });

        return model;
    }

    angular
        .module("settings")
        .factory("productRolesFormConfig", [
            "baseFormConfig",
            "rpFormInputTextConfig",
            factory
        ]);
})(angular);

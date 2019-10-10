// Product Item Properties Access Form Configuration

(function (angular) {
    "use strict";

    function factory(baseFormConfig, inputTextConfig) {
        var model = baseFormConfig();

        model.searchText = inputTextConfig({
            fieldName: "propertiesSearchText",
            iconClass: "rp-icon-search2",
            onChange: model.getMethod("filterProperties")
        });

        return model;
    }

    angular
        .module("settings")
        .factory("productPropertiesFormConfig", [
            "baseFormConfig",
            "rpFormInputTextConfig",
            factory
        ]);
})(angular);

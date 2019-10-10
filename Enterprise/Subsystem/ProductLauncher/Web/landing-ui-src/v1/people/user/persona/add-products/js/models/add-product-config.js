//  Persona - Add Product Form Config Model

(function (angular) {
    "use strict";

    function factory(baseFormConfig, inputTextConfig) {
        var model = baseFormConfig();

        model.searchText = inputTextConfig({
        	id: "searchText",
        	fieldName: "searchText",
            iconClass: "rp-icon-search2",
            onChange: model.getMethod("clearSelectAllProducts")
        });

        model.selectAll = {
            id: "selectAllProducts"
        };

        return model;
    }

    angular
        .module("settings")
        .factory("addProductFormConfig", [
            "baseFormConfig",
        	"rpFormInputTextConfig",
        	factory
        ]);
})(angular);

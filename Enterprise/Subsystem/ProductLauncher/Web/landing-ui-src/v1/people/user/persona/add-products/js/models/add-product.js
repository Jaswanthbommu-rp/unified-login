//  Add Product Form Model

(function (angular) {
    "use strict";

    function factory(baseForm) {
        var model = baseForm();

        model.isSelectAll = function() {
            return model.form.selectAll;
        };

        model.setSelectAll = function(flag) {
            model.form.selectAll = flag;
        };

        model.clearFilter = function() {
            model.form.searchObj.model.name = "";
        };

        return model;
    }
 
    angular
        .module("settings")
        .factory("addProductFormModel", [
            "baseForm",
        	factory
        ]);
})(angular);

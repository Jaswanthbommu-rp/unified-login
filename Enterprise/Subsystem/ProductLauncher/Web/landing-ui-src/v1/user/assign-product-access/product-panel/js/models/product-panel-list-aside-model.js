//  List Aside Model

(function (angular) {
    "use strict";

    function factory() {
        var model = {};

        model.getName = function () {
            return model.Name;
        };

        model.getListID = function () {
            return model.ID;
        };

        model.getProductID = function () {
            return model.ProductId;
        };

        model.setName = function (name) {
            model.Name = name;
        };

        model.setListID = function (id) {
            model.ID = id;
        };

        model.setProductID = function (productId) {
            model.ProductId = productId;
        };

        model.reset = function () {
            model = {};
        };

        return model;
    }

    angular
        .module("settings")
        .factory("productPanelListAsideModel", [
            factory
        ]);
})(angular);

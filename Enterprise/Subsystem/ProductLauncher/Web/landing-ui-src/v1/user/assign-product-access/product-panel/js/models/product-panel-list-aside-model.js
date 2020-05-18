//  List Aside Model

(function (angular) {
    "use strict";

    function factory() {
        var model = {};
        var productLists = [1, 3]; // list of panels don't require the footer buttons

        model.getName = function () {
            return model.Name;
        };

        model.getListID = function () {
            return model.ID;
        };

        model.getProductID = function () {
            return model.ProductId;
        };

        model.getTabName = function () {
            return model.tabName;
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

        model.setTabName = function (name) {
            model.tabName = name;
        };

        model.FooterRequired = function (productId){
            var flag = true;
            productLists.forEach(function (id) {
                if(productId == id){
                   flag = false;
                }
            });
            return flag;
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

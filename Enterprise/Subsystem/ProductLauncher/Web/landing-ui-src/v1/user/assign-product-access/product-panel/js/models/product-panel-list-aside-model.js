//  List Aside Model

(function (angular) {
    "use strict";

    function factory() {
        var model = {};
        var productLists = [44]; // list of panels require the footer buttons
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

        model.getRoleType = function () {
            return model.roleType;
        };

        model.getSelectedPropertyRoleData = function () {
            return model.propertyData;
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

        model.setRoleType = function (name) {
            model.roleType = name;
        };
        
        model.setSelectedPropertyRoleData = function (data) {
            model.propertyData = data;
        };
      
        model.FooterRequired = function (productId){
            var flag = false;
            productLists.forEach(function (id) {
                if(productId == id){
                   flag = true;
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

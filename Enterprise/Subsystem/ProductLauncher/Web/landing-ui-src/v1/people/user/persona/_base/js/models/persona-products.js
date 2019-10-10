//  Persona - Add Product Data Model

(function (angular) {
    "use strict";

    function factory() {
        var model = {},
            data = {
                productList: [], //main source of products
                families: [] //list of products sorted for view
            };

        model.data = data;

        // Setters

        model.setProducts = function(arr) {
            data.productList = data.productList.concat(arr);
        };

        model.setFamilies = function(arr) {
            data.families = data.families.concat(arr);
        };

        // Getters

        model.getProducts = function() {
            return data.productList;
        };

        model.getFamilies = function() {
            return data.families;
        };

        // Assertions

        model.hasFamilies = function() {
            return data.families && data.families.length > 0;
        };

        model.hasProducts = function() {
            return data.productList && data.productList.length > 0;
        };

        model.hasSelectedProducts = function() {
            for(var i=0, max=data.productList.length; i<max; i++) {
                if(data.productList[i].isSelected()) {
                    return true;
                }
            }

            return false;
        };

        // Actions

        model.openFirstProduct = function() {
            // data.productList[0].data.displayPanel = true;
        };

        model.reset = function() {
            data.productList.flush();
            data.families.flush();
        };

        return model;
    }

    angular
        .module("settings")
        .factory("personaProducts", [
            factory
        ]);
})(angular);
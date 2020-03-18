//  change Order  Model

(function(angular) {
    "use strict";

    function factory(pubsub, user, svc, prodConfig, persona) {
        var model = {};

        model.init = function() {
            model.isDefaultProd = true;
            model.copyData = {};
            model.data = {};

            return model;
        };

        model.getProdData = function() {
            var params = { editorPersonaId: persona.getId() };

            svc.getData(params)
                .then(model.setData, model.setDataErr);
        };

        model.setData = function(data) {
            model.setAllProducts(data);
            model.getAllProductsOptions();
            model.setMenuOptions();
            model.setDefaultProduct();
            pubsub.publish("onesiteSettings.setSelMenu");
        };

        model.setDataErr = function(data) {
            logc("Error: ", data);
        };

        model.setAllProducts = function(data) {
            model.data = data;
        };

        model.getAllProducts = function() {
            return model.data;
        };

        model.setSelProduct = function(data) {
            model.prod = data;
        };

        model.getSelProduct = function() {
            return model.prod;
        };


        model.getAllProductsOptions = function() {
            var data = model.getAllProducts();
            var dataOptions = [];

            var defaultvalue = {
                productName: "All",
                productVal: "All"
            };

            dataOptions.push(defaultvalue);
            if (data.records !== undefined) {
                data.records.forEach(function(item) {
                    var o = {
                        productName: item,
                        productVal: item
                    };
                    dataOptions.push(o);
                });
            }

            return dataOptions;
        };

        model.setMenuOptions = function() {
            var data = model.getAllProductsOptions();
            prodConfig
                .setOptions("optionsData", data);
        };

        model.setDefaultProduct = function() {
            var data = model.getAllProductsOptions();
            model.setSelProduct(data[0].productVal);
        };


        return model.init();
    }

    angular
        .module("settings")
        .factory("onesiteProductsSelectMenu", [
            "pubsub",
            "userSessionModel",
            "onesiteProductsSvc",
            "onesiteProductsConfig",
            "personaDetails",
            factory
        ]);
})(angular);
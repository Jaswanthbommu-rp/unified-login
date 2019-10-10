//  change Order  Model

(function(angular) {
    "use strict";

    function factory(pubsub, user, svc, prodConfig, persona) {
        var model = {};

        model.init = function() {
            model.isDefaultProd = true;
            model.copyData = {};
            model.data = {};
            model.isReady = false;
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
            model.isReady = true;
            pubsub.publish("acctSettings.setSelMenu");
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
        .factory("acctProductsSelectMenu", [
            "pubsub",
            "userSessionModel",
            "acctProductsSvc",
            "acctProductsConfig",
            "personaDetails",
            factory
        ]);
})(angular);
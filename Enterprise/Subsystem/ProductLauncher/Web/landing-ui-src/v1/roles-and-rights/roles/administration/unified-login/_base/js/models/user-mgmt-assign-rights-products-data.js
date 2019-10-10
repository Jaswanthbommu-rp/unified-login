//  products  Model

(function(angular) {
    "use strict";

    function factory(user, persona) {
        var model = {};

        model.init = function() {
            model.isReady = false;
            return model;
        };

        model.getProdData = function(svc) {
            var params = { editorPersonaId: persona.getId() };

            svc.getData(params)
                .then(model.setData, model.setDataErr);
        };

        model.setData = function(data) {
            model.setProducts(data);
            model.isReady = true;
        };

        model.setDataErr = function(data) {
            logc("Error: ", data);
        };

        model.setProducts = function(data) {
            model.data = data;
        };

        model.getProducts = function() {
            return model.data;
        };

        model.getProductsData = function() {
            var data = model.getProducts();
            return model.getAllProductsOptions(data);
        };

        model.getAllProductsOptions = function(data) {

            var dataOptions = [];
            if (model.isReady === true) {

                var def = {
                    value: "",
                    name: "All"
                };

                dataOptions.push(def);

                if (data.records !== undefined) {
                    data.records.forEach(function(item) {
                        var o = {
                            value: item,
                            name: item
                        };

                        dataOptions.push(o);
                    });
                }
            }

            return dataOptions;
        };


        return model.init();
    }

    angular
        .module("settings")
        .factory("userMgmtProductsData", [
            "userSessionModel",
            "personaDetails",
            factory
        ]);
})(angular);
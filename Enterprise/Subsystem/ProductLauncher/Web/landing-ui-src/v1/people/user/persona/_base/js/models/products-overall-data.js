//  User Overall Data Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function ProductsOverallDataModel() {
            var s = this;
            s.init();
        }

        var p = ProductsOverallDataModel.prototype;

        p.init = function () {
            var s = this;
            s.models = [];
            s.registered = {};
        };

        p.register = function (name, model) {
        	var s = this;
        	if (!s.registered[name]) {
        		s.models.push(model);
        		s.registered[name] = true;
        	}
        	return s;
        };

        p.getData = function () {
        	var s = this,
        		data = {
                    productBatch: []
                };

        	s.models.forEach(function (model) {
                var modelData= model.getData();

                if(modelData) {
        	       data.productBatch.push(modelData);
                }
        	});

        	return data;
        };

        p.reset = function () {
            var s = this;
            s.models.flush();
            s.registered = {};
        };

        return new ProductsOverallDataModel();
    }

    angular
        .module("settings")
        .factory("productsOverallDataModel", [factory]);
})(angular);

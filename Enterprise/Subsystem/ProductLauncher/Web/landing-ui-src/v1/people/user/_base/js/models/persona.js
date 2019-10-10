//  User Persona Model

(function (angular) {
    "use strict";

    function factory() {
        function PersonaModel() {
            var s = this;
            s.init();
        }

        var p = PersonaModel.prototype;

        p.init = function () {
            var s = this;

            s.data = {
                personaId: 0,
                name: null,
                type: 1,
                startDate: null,
                endDate: null,
                products: [],

                defaultName: null,
                tabID: null
            };

            s.isActive = true;
            s.isEdited = false;
        };

        p.setData = function(data) {
            var s = this;
            angular.extend(s.data, data || {});

            return s;       
        };

        p.setDirty = function(flag) {
            var s = this;
            s.isEdited = flag;

            return s;
        };

        p.deactivate = function() {
            var s = this;
            s.isActive = false;
            
            return s;
        };

        p.getName = function() {
            var s = this;
            return s.data.name;
        };

        p.destroyProduct = function(product) {
            product.destroy();
        };

        p.destroy = function () {
            var s = this;

            s.data.products.forEach(s.destroyProduct);
            s.data.products.flush();
            s.data.products = undefined;

            s.data.startDate = undefined;
            s.data.endDate = undefined;
            s.data = undefined;
        };


        return function (data) {
            return (new PersonaModel()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("personaModel", [
            factory
        ]);
})(angular);

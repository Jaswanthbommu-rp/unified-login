//  Product Item

(function (angular) {
    "use strict";

    function factory() {
        function ProductItemModel() {
            var s = this;
            s.init();
        }

        var p = ProductItemModel.prototype;

        p.init = function () {
            var s = this;

            s.data = {
                id: "",
                name: "",
                subsolution: "",

                productFamily: -1,
            };

            s.state = {
                isSelected: false
            };
        };

        // Setters

        p.setData = function (data) {
            var s = this;
            angular.extend(s.data, data || {});
            return s;
        };

        p.setSelected = function(flag) {
            var s = this;
            s.state.isSelected = flag;
            return s;
        };

        // Getters

        p.getFamilyId = function() {
            var s = this;
            return s.data.productFamily;
        };

        p.getId = function() {
            var s = this;
            return s.data.id;
        };

        p.getName = function() {
            var s = this;
            return s.data.name;
        };

        // Assertions

        p.isSelected = function() {
            var s = this;
            return s.state.isSelected;
        };

        // Actions

        p.destroy = function() {
            var s = this;
            s.data = undefined;
            s = undefined;
        };

        return function(data) {
            return (new ProductItemModel()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("personaProductItem", [
            factory
        ]);

})(angular);
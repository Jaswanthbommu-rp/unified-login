//  Assign Product Access Model

(function (angular, undefined) {
    "use strict";

    function factory(pubsub, templateModel) {
        function AssignProductAccessModel() {
            var s = this;
            s.init();
        }

        var p = AssignProductAccessModel.prototype;

        p.init = function () {
            var s = this;
            s.products = {};
        };

        // Getters

        p.getAccessData = function (key, productId) {
            var s = this;

            if (templateModel.isProductExists(productId)) {
                return s.products[key].getData(productId);
            }
            else {
                return s.products[key].getData();
            }

            return undefined;
        };

        // Actions

        p.register = function (data) {
            var s = this;
            logc("register model", data);
            s.products[data.key] = data.model;
            return s;
        };

        p.selectSoln = function (soln) {
            var s = this,
                found = false,
                key = soln.getKey();

            angular.forEach(s.products, function (item, itemKey) {
                var active = key == itemKey;

                if (active) {
                    found = true;
                }

                item.setActive(active);
            });


            s.products["default"].setActive(!found);
            pubsub.publish("product.selectedProduct", soln.data);

            return s;
        };

        // Assertions

        p.hasAccessData = function (key) {
            var s = this;
            return s.products[key].hasData();
        };

        p.accessChanged = function (key) {
            var s = this;

            if (s.products[key]) {
                return s.products[key].hasChanged();
            }

            return false;
        };

        // Reset/Destroy

        p.reset = function () {
            var s = this;

            angular.forEach(s.products, function (product) {
                product.reset();
            });
        };

        return new AssignProductAccessModel();
    }

    angular
        .module("settings")
        .factory("assignProductAccessModel", ["pubsub", "productTemplateModel", factory]);
})(angular);

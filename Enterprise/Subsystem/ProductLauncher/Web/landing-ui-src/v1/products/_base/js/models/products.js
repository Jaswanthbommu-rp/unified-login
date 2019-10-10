//  Products Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function Products() {
            var s = this;
            s.init();
        }

        var p = Products.prototype;

        p.init = function () {
            var s = this;
            s.families = [];
            s.solutions = [];
            s.hasSolutions = true;
        };

        // Setters

        p.setFamilies = function (list) {
            var s = this;
            s.families = list;
            return s;
        };

        p.setSolutions = function (list) {
            var s = this;
            s.solutions = list;
            return s;
        };

        // Getters

        p.getFamilyFilterOptions = function () {
            var s = this,
                options = [];

            s.families.forEach(function (family) {
                options.push({
                    value: family.getId(),
                    name: family.getFamilyName()
                });
            });

            return options;
        };

        p.getSolutionFilterOptions = function () {
            var s = this,
                options = [];

            s.solutions.forEach(function (soln) {
                options.push({
                    value: soln.getId(),
                    famId: soln.getFamilyId(),
                    name: soln.getSolutionName()
                });
            });

            return options;
        };

        // Actions

        p.updateDisplay = function (filter) {
            var s = this,
                count = 0;

            s.families.forEach(function (family) {
                if (family.hasRelevantSolutions(filter)) {
                    count++;
                }
            });

            s.hasSolutions = count !== 0;

            return s;
        };

        p.reset = function () {
            var s = this;
            s.families.flush();
            s.solutions.flush();
        };

        return new Products();
    }

    angular
        .module("settings")
        .factory("productsModel", [factory]);
})(angular);

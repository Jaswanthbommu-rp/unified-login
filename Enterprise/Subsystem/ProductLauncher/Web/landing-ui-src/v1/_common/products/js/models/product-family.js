//  Product Family Model

(function (angular, undefined) {
    "use strict";

    function factory($filter) {
        function ProductFamilyModel() {
            var s = this;
            s.init();
        }

        var p = ProductFamilyModel.prototype;

        p.init = function () {
            var s = this;
            s.data = {};
            s.solutions = [];
            s.isActive = true;
        };

        // Setters

        p.setData = function (data) {
            var s = this;

            var familyName = data.family ? data.family : data.familyName;

            s.data = {
                familyId: data.familyId,
                familyName: familyName
            };

            return s;
        };

        p.setSolutions = function (solns) {
            var s = this;
            s.solutions = solns;
            return s;
        };

        // Getters

        p.getData = function () {
            var s = this;
            return s.data;
        };

        p.getFamilyName = function () {
            var s = this;
            return s.data.familyName;
        };

        p.getIconId = function () {
            var s = this;
            return "FAM" + s.data.familyId;
        };

        p.getId = function () {
            var s = this;
            return s.data.familyId;
        };

        p.getSolutions = function (filter) {
            var s = this,
                solns = [].concat(s.solutions);

            if (filter) {
                solns = s.solutions.filter(function (soln) {
                    switch (filter) {
                        case "appSwitcher":
                            return soln.getAppSwitcherStatus();
                        case "productFilter":
                            return soln.getProductFilterStatus();
                    }
                });
            }
            return solns;
        };

        // Actions

        p.addSolution = function (solution) {
            var s = this;
            s.solutions.push(solution);
            return s;
        };

        // Assertions

        p.hasId = function (id) {
            var s = this;
            return s.data.familyId === id;
        };

        p.hasRelevantSolutions = function (filter) {
            var s = this,
                count = 0;

            s.solutions.forEach(function (soln) {
                if (soln.isRelevant(filter)) {
                    count++;
                }
            });

            s.isActive = count !== 0;

            return s.isActive;
        };

        // Destroy/Reset

        p.destroySolution = function (solution, index) {
            var s = this;
            solution.destroy();
            delete s.solutions[index];
        };

        p.destroy = function () {
            var s = this;
            s.solutions.forEach(s.destroySolution.bind(s));
            s.data = undefined;
            s.solutions = undefined;
        };

        return function (data) {
            return (new ProductFamilyModel()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("productFamilyModel", ["$filter", factory]);
})(angular);

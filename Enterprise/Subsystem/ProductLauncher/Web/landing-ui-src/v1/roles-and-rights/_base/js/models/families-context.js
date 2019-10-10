//  Product Family Model

(function(angular, undefined) {
    "use strict";

    function factory($filter, sideMenuModel) {
        function ProductFamilyModel() {
            var s = this;
            s.init();
        }

        var p = ProductFamilyModel.prototype;

        p.init = function() {
            var s = this;
            s.data = {};
            s.solutions = [];
            s.isActive = true;
            s.sideMenuList = sideMenuModel();
        };

        // Setters

        p.setData = function(data) {
            var s = this;

            s.data = {
                familyId: data.familyId,
                familyName: data.title
            };

            return s;
        };

        // Getters

        p.getFamilyName = function() {
            var s = this;
            return s.data.familyName;
        };

        p.getIconId = function() {
            var s = this;
            return "FAM" + s.data.familyId;
        };

        p.getId = function() {
            var s = this;
            return s.data.familyId;
        };

        // Actions

        p.addSolution = function(solution) {
            var s = this;
            s.solutions.push(solution);
            return s;
        };

        // Assertions

        p.hasId = function(id) {
            var s = this;
            return s.data.familyId === id;
        };

        p.hasRelevantSolutions = function(filter) {
            var s = this,
                count = 0;

            s.solutions.forEach(function(soln) {
                if (soln.isRelevant(filter)) {
                    count++;
                }
            });

            s.isActive = count !== 0;

            return s.isActive;
        };

        // Destroy/Reset

        p.destroySolution = function(solution, index) {
            var s = this;
            solution.destroy();
            delete s.solutions[index];
        };

        p.destroy = function() {
            var s = this;
            s.solutions.forEach(s.destroySolution.bind(s));
            s.data = undefined;
            s.solutions = undefined;
        };

        return function(data) {
            return (new ProductFamilyModel()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("productFamilyModelExt", ["$filter", "solSideMenuModel", factory]);
})(angular);
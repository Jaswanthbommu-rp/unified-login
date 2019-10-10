//  Assign Product Family Model

(function (angular, undefined) {
    "use strict";

    function factory($filter, selectAll, solnModel) {
        var index = 0;

        function AssignProductFamilyModel() {
            var s = this;
            index++;
            s.init();
        }

        var p = AssignProductFamilyModel.prototype;

        p.init = function () {
            var s = this;
            s.data = {};
            s.solutions = [];
            s.id = "fam" + index;
            s.selectAll = selectAll();
        };

        // Getters

        p.getSolns = function () {
            var s = this;
            return s.solutions;
        };

        p.getAssignedSolns = function () {
            var s = this,
                solns = [];

            s.solutions.forEach(function (soln) {
                if (soln.isAssigned()) {
                    solns.push(soln);
                }
            });

            return solns;
        };

        p.getTitleText = function () {
            var s = this,
                key = "text.familyTitle." + s.data.familyId;
            return $filter("assignProductsText")(key);
        };

        // Setters

        p.setData = function (data) {
            var s = this,
                list = [];

            s.data = data || {};

            (data.solutions || []).forEach(function (solnData) {
                var soln = solnModel(solnData);
                s.solutions.push(soln);
                list.push(soln.getSelectItem());
            });

            s.selectAll.setList(list);

            return s;
        };

        // Actions

        p.findSolnById = function (id) {
            var soln,
                s = this;

            s.solutions.forEach(function (item) {
                if (!soln && item.hasId(id)) {
                    soln = item;
                }
            });

            return soln;
        };

        p.selectSoln = function (soln) {
            var s = this;

            s.solutions.forEach(function (item) {
                item.setSelected(item.getId() == soln.getId());
            });

            return s;
        };

        // Destroy/Reset

        p.destroySolution = function (soln) {
            soln.destroy();
        };

        p.destroy = function () {
            var s = this;
            s.solutions.forEach(s.destroySolution);
            s.solutions.flush();

            s.data = undefined;
            s.solutions = undefined;
        };

        return function (data) {
            return (new AssignProductFamilyModel()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("assignProductFamilyModel", [
            "$filter",
            "rpSelectAllModel",
            "assignProductSolutionModel",
            factory
        ]);
})(angular);

//  Products Data Model

(function(angular, undefined) {
    "use strict";

    function factory($filter, eventStream, user, dataSvc, productFamily, productSolution) {
        function ProductsDataModel() {
            var s = this;
            s.init();
        }

        var p = ProductsDataModel.prototype;

        p.init = function() {
            var s = this;

            s.data = [];
            s.ready = false;
            s.productFamilies = [];
            s.productSolutions = [];

            s.events = {
                update: eventStream()
            };

            s.loadData();

        };

        // Setters

        p.setData = function(resp) {
            var s = this;
            if (resp.status.success) {
                s.ready = true;
                s.data = resp.data;
                s.assembleFamilies();
                s.events.update.publish(resp.data);
            }
            return s;
        };

        // Getters

        p.getData = function() {
            var s = this;
            return s.data;
        };

        p.getFamilies = function() {
            var s = this,
                list = [].concat(s.productFamilies);

            return $filter("orderBy")(list, "data.familyId");
        };

        p.getSolutions = function() {
            var s = this;
            return [].concat(s.productSolutions);
        };

        // Actions

        p.assembleFamilies = function() {
            var s = this,
                store = {};

            s.productFamilies = [];
            s.productSolutions = [];


            s.data.forEach(function(fam) {
                var soln,
                    key = "fam" + fam.familyId;


                if (!store[key]) {
                    store[key] = productFamily(fam);
                    s.productFamilies.push(store[key]);
                }


                fam.solutions.forEach(function(sol) {
                    var soln = productSolution(sol);
                    store[key].addSolution(sol);
                    s.productSolutions.push(sol);
                });

            });

            return s;
        };

        p.loadData = function() {
            var params,
                s = this;

            params = {
                personRealPageId: user.data.realPageId,
                accessFilter: "RolesAndRights"
            };

            dataSvc.get(params, s.setData.bind(s));
        };

        p.subscribe = function(callback) {
            var s = this;
            return s.events.update.subscribe(callback);
        };

        // Assertions

        p.isReady = function() {
            var s = this;
            return s.ready;
        };

        // Destroy/Reset

        p.reset = function() {
            var s = this;

            return s;
        };

        return new ProductsDataModel();
    }

    angular
        .module("settings")
        .factory("productsDataModelExt", [
            "$filter",
            "eventStream",
            "userSessionModel",
            "rolesAndRightsAssignProductsSvc",
            "productFamilyModelExt",
            "productSolutionModelExt",
            factory
        ]);
})(angular);

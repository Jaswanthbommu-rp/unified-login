//  Products Data Model

(function (angular, undefined) {
    "use strict";

    function factory($filter, eventStream, persona, dataSvc, productFamily, productSolution, pubsub) {
        function ProductsDataModel() {
            var s = this;
            s.init();
        }

        var p = ProductsDataModel.prototype;

        p.init = function () {
            var s = this;

            s.data = [];
            s.ready = false;
            s.productFamilies = [];
            s.productSolutions = [];

            s.events = {
                update: eventStream()
            };

            if (persona.isReady()) {
                s.loadData();
            }
            else {
                s.personaWatch = persona.subscribe(s.loadData.bind(s));
            }

            s.reloadWatch = pubsub.subscribe("productsData.reload", s.loadData.bind(s));
        };

        // Setters

        p.setData = function (resp) {
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

        p.getData = function () {
            var s = this;
            return s.data;
        };

        p.getFamilies = function () {
            var s = this,
                list = [].concat(s.productFamilies);

            return $filter("orderBy")(list, "data.familyId");
        };

        p.getSolutions = function (filter) {
            var solns,
                s = this;

            if (filter) {
                solns = s.productSolutions.filter(function (soln) {
                    switch (filter) {
                        case "appSwitcher":
                            return soln.getAppSwitcherStatus();
                        case "productFilter":
                            return soln.getProductFilterStatus();
                    }
                });
            }
            else {
                solns = [].concat(s.productSolutions);
            }

            return solns;
        };

        // Actions

        p.assembleFamilies = function () {
            var s = this,
                store = {};

            s.productFamilies = [];
            s.productSolutions = [];

            if (s.data) {
                s.data.forEach(function (item) {
                    var soln = productSolution(item),
                        key = "fam" + item.familyId;

                    if (!item.hasAccess) {
                        return;
                    }

                    if (!store[key]) {
                        store[key] = productFamily(item);
                        s.productFamilies.push(store[key]);
                    }

                    store[key].addSolution(soln);
                    s.productSolutions.push(soln);
                });
            }

            return s;
        };

        p.loadData = function () {
            var params,
                s = this;

            params = {
                mergePersonaAccess: true,
				allProducts: false,
                realPageId: persona.getOrgRealPageID()
            };

            dataSvc.get(params, s.setData.bind(s));
        };

        p.subscribe = function (callback) {
            var s = this;
            return s.events.update.subscribe(callback);
        };

        // Assertions

        p.isReady = function () {
            var s = this;
            return s.ready;
        };

        // Destroy/Reset

        p.reset = function () {
            var s = this;

            return s;
        };

        return new ProductsDataModel();
    }

    angular
        .module("settings")
        .factory("productsDataModel", [
            "$filter",
            "eventStream",
            "personaDetails",
            "productsDataSvc",
            "productFamilyModel",
            "productSolutionModel",
            "pubsub",
            factory
        ]);
})(angular);

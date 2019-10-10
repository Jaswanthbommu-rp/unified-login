//  Roles and Rights Products Model

(function(angular, undefined) {
    "use strict";

    function factory(eventStream) {
        function RolesAndRightsProducts() {
            var s = this;
            s.init();
        }

        var p = RolesAndRightsProducts.prototype;

        p.init = function() {
            var s = this;
            s.ready = false;
            s.families = [];
            s.solutions = [];
            s.hasSolutions = true;
            s.selSol = "";
            s.events = {
                update: eventStream()
            };
        };

        // Setters

        p.setFamilies = function(list) {
            var s = this;
            s.families = list;
            s.ready = true;
            s.events.update.publish(list);
            return s;
        };

        p.getFamilies = function() {
            var s = this;
            return s.families;

        };

        p.setSolutions = function(list) {
            var s = this;
            s.solutions = list;
            return s;
        };

        // Getters

        p.getFamilyFilterOptions = function() {
            var s = this,
                options = [];

            s.families.forEach(function(family) {
                options.push({
                    value: family.getId(),
                    name: family.getFamilyName()
                });
            });

            return options;
        };

        p.getSolutionFilterOptions = function() {
            var s = this,
                options = [];

            s.solutions.forEach(function(soln) {
                options.push({
                    value: soln.getId(),
                    famId: soln.getFamilyId(),
                    name: soln.getSolutionName()
                });
            });

            return options;
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
        // Actions

        p.updateDisplay = function(filter) {
            var s = this,
                count = 0;

            s.families.forEach(function(family) {
                if (family.hasRelevantSolutions(filter)) {
                    count++;
                }
            });

            s.hasSolutions = count !== 0;

            return s;
        };

        p.setActiveSol = function(sol) {
            var s = this;
            s.selSol = sol;
        };

        p.getActiveSol = function() {
            var s = this;
            return s.selSol;
        };

        p.setFamilyAccordian = function(sol) {
            var s = this;
            var families = s.getFamilies();
            
            families.forEach(function(fam) {
                fam.isActive = false;
                fam.solutions.forEach(function(famsol) {
                    if (famsol.solutionId === sol.solutionId) {
                        fam.isActive = true;
                        return;
                    }
                });

            });
        };


        p.reset = function() {
            var s = this;

            return s;
        };

        return new RolesAndRightsProducts();
    }

    angular
        .module("settings")
        .factory("rolesAndRightsProductsModel", ["eventStream", factory]);
})(angular);
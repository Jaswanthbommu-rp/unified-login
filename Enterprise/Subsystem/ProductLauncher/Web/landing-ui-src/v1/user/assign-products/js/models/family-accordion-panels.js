//  Family Accordion Panels Model

(function (angular, undefined) {
    "use strict";

    function factory(timeout) {
        function FamilyAccordionPanels() {
            var s = this;
            s.init();
        }

        var p = FamilyAccordionPanels.prototype;

        p.init = function () {
            var s = this;
            s.panels = {};
            s.ready = false;
        };

        p.register = function (data) {
            var s = this;
            s.panels[data.name] = data.panel;
            return s;
        };

        p.remove = function (data) {
            var s = this;
            delete s.panels[data.name];
            return s;
        };

        p.initState = function () {
            var s = this;

            angular.forEach(s.panels, function (panel) {
                timeout(panel.initState.bind(panel), 200);
            });

            return s;
        };

        p.reset = function () {
            var s = this;
            s.panels = {};
        };

        return new FamilyAccordionPanels();
    }

    angular
        .module("settings")
        .factory("familyAccordionPanels", ["timeout", factory]);
})(angular);

//  Coming Soon Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function ComingSoonProductAccessModel() {
            var s = this;
            s.init();
        }

        var p = ComingSoonProductAccessModel.prototype;

        p.init = function () {
            var s = this;
        };

        p.setActive = function (bool) {
            var s = this;
            s.active = bool;
            return s;
        };

        p.isActive = function () {
            var s = this;
            return s.active;
        };

        p.hasChanged = function () {
            var s = this;
            return false;
        };

        p.reset = function () {
            var s = this;
            s.active = false;
        };

        return new ComingSoonProductAccessModel();
    }

    angular
        .module("settings")
        .factory("comingSoonProductAccessModel", [factory]);
})(angular);

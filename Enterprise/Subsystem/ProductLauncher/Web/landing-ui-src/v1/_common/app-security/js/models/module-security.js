//  Module Security Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function ModuleSecurity() {
            var s = this;
            s.init();
        }

        var p = ModuleSecurity.prototype;

        p.init = function () {
            var s = this;
            s.data = {};
        };

        p.setData = function (data) {
            var s = this;
            s.data = data || {};
            return s;
        };

        p.hasAccess = function () {
            var s = this;
            return s.data.hasAccess === true;
        };

        p.reset = function () {
            var s = this;
            s.data = {};
        };

        p.destroy = function () {
            var s = this;
            s.data = undefined;
        };

        return function (data) {
            return (new ModuleSecurity()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("moduleSecurityModel", [factory]);
})(angular);

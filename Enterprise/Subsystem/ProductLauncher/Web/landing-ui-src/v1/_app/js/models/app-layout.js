//  App Layout Model

(function (angular, undefined) {
    "use strict";

    function factory(layout) {
        function AppLayoutModel() {
            var s = this;
            s.init();
        }

        var p = AppLayoutModel.prototype;

        p.init = function () {
            var s = this;

            s.data = {
                "appNav": true,
                "appHeader": true,
                "appFooter": true,
                "appSubheader": true
            };

            return s;
        };

        p.getData = function () {
            var s = this;
            return s.data;
        };

        p.set = function (name, bool) {
            var s = this;

            if (s.data[name] === undefined) {
                logc("AppLayoutModel.set: %s is not a valid page region.", name);
            }
            else {
                s.data[name] = bool;
            }

            return s;
        };

        p.setList = function (list, bool) {
            var s = this;

            list.forEach(function (name) {
                s.set(name, bool);
            });

            return s;
        };

        p.hide = function (keys) {
            var s = this,
                isList = keys && keys.push;
            s[isList ? "setList" : "set"](keys, false);
            return s;
        };

        p.show = function (keys) {
            var s = this,
                isList = keys && keys.push;
            s[isList ? "setList" : "set"](keys, true);
            return s;
        };

        return new AppLayoutModel();
    }

    angular
        .module("settings")
        .factory("appLayoutModel", [factory]);
})(angular);

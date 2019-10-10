//  Role Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function Role() {
            var s = this;
            s.init();
        }

        var p = Role.prototype;

        p.init = function () {
            var s = this;
            s.id = "";
            s.isAssigned = false;
            s.roletype = "";
            s.name = "";
            s.disabled = false;
            s.hideProperties = false;
        };

        p.getId = function () {
            var s = this;
            return s.id;
        };

        p.getAssigned = function () {
            var s = this;
            return s.isAssigned;
        };

        p.getName = function () {
            var s = this;
            return s.name;
        };

        p.hidePropertiesTab = function () {
            var s = this;
            return s.hideProperties;
        };

        p.setData = function (data) {
            var s = this;
            s.id = data.id;
            s.isAssigned = data.isAssigned;
            s.disabled = data.disabled;
            s.roletype = data.roletype;
            s.name = data.name;
            s.hideProperties = data.accessAllProperties;
            return s;
        };

        p.hasId = function (val) {
            var s = this;
            return s.id === val;
        };

        p.setAssigned = function (val) {
            var s = this;
            s.isAssigned = val;
            return s;
        };

        p.isRoleAssigned = function () {
            var s = this;
            return s.isAssigned;
        };

        p.reset = function () {
            var s = this;
            s.id = "";
            s.isAssigned = false;
            s.roletype = "";
            s.name = "";
        };

        return function (data) {
            return (new Role()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("cpCompRoleModel", [
            factory
        ]);
})(angular);
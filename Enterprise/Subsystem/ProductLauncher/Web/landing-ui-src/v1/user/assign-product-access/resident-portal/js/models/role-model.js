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
            s.assigned = false;
            s.disabled = false;
            s.name = "";
        };

        p.getId = function () {
            var s = this;
            return s.id;
        };

        p.getAssigned = function () {
            var s = this;
            return s.assigned;
        };

        p.getName = function () {
            var s = this;
            return s.name;
        };

        p.setData = function (data) {
            var s = this;
            s.id = data.id;
            s.assigned = data.isAssigned;
            s.disabled = (data.disabled || data.isDisabled);
            s.name = data.name;

            return s;
        };

        p.hasId = function (val) {
            var s = this;
            return s.id === val;
        };

        p.setAssigned = function (val) {
            var s = this;
            s.assigned = val;
            return s;
        };

        p.isAssigned = function () {
            var s = this;
            return s.assigned;
        };

        p.isEnterprise = function () {
            var s = this;
            return s.id.indexOf("ENTERPRISE") !== -1;
        };

        p.isEnterpriseAdmin = function () {
            var s = this;
            return s.id === "ENTERPRISEADMIN";
        };

        p.isStaff = function () {
            var s = this;
            return s.id.indexOf("STAFF") !== -1;
        };

        p.reset = function () {
            var s = this;
            s.id = "";
            s.assigned = false;
            s.disabled = false;
            s.name = "";
        };

        return function (data) {
            return (new Role()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("roleModel", [
            factory
        ]);
})(angular);

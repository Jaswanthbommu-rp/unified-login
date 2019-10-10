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
            s.name = "";
            s.roleType = "";
            s.disableSelection = false;
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
            s.disableSelection = data.disableSelection;
            s.name = data.name;
            if (data.roletype) {
                s.roleType = data.roletype;
            }
            /////
            if (data.id == "70750") {
                s.roleType = "Department Name";
            }
            ////
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

        p.reset = function () {
            var s = this;
            s.id = "";
            s.assigned = false;
            s.name = "";
        };

        return function (data) {
            return (new Role()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("dmRoleModel", [
            factory
        ]);
})(angular);

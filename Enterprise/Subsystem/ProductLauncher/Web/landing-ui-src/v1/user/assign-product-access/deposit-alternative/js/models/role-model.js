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
            s.name = "";
            s.disabled = false;
            
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

        
        p.setData = function (data) {
            var s = this;
            s.id = data.id;            
            s.isAssigned = data.isAssigned;
            s.disabled = data.disabled === undefined ? false : data.disabled;            
            s.name = data.name;            
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
            s.name = "";
        };

        return function (data) {
            return (new Role()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("daRoleModel", [
            factory
        ]);
})(angular);
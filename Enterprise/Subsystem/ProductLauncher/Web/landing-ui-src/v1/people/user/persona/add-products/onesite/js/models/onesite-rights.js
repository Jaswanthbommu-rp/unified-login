//  Rights List Model

(function (angular) {
    "use strict";

    function factory() {
        var model = {};

        model.getName = function () {
            return model.roleName;
        };

        model.getRoleID = function () {
            return model.roleID;
        };

        model.setName = function (name) {
            model.roleName = name;
        };

        model.setRoleID = function (roleID) {
            model.roleID = roleID;
        };

        model.reset = function () {
            model = {};
        };

        return model;
    }

    angular
        .module("settings")
        .factory("onesiteRightsModel", [
            factory
        ]);
})(angular);

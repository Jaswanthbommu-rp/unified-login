//  Properties List Model

(function (angular) {
    "use strict";

    function factory() {
        var model = {};

        model.getName = function () {
            return model.groupName;
        };

        model.getPropertyGroupID = function () {
            return model.groupId;
        };

        model.setName = function (name) {
            model.groupName = name;
        };

        model.setPropertyGroupID = function (groupId) {
            model.groupId = groupId;
        };

        model.reset = function () {
            model = {};
        };

        return model;
    }

    angular
        .module("settings")
        .factory("paPropertiesModel", [
            factory
        ]);
})(angular);

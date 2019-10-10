//  Property Group Model

(function (angular) {
    "use strict";

    function factory() {
        var model = {};

        model.getName = function () {
            return model.propertyGroupName;
        };

        model.getPropertyGroupID = function () {
            return model.propertyGroupID;
        };

        model.setName = function (name) {
            model.propertyGroupName = name;
        };

        model.setPropertyGroupID = function (id) {
            model.propertyGroupID = id;
        };

        model.reset = function () {
            model = {};
        };

        return model;
    }

    angular
        .module("settings")
        .factory("biPropertyGroupModel", [
            factory
        ]);
})(angular);

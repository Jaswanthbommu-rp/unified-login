(function (angular, undefined) {
    "use strict";

    function factory(
        tabsContext,
        roleConfig
    ) {
        var model = {};

        model.init = function () {
            model.propertyData = {};
            return model;
        };

        model.setPropertyData = function () {
            model.propertyData = tabsContext.get().data;
            return model;
        };

        model.setData = function (data) {
            model.data = data;
        };

        model.getData = function () {
            return model.data;
        };

        model.isReady = function () {
            return model.propertyData.properties.length > 0;
        };

        model.reset = function () {
            model.propertyData = {};
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("rmPropertyAssignModel", [
            "rmAssignPropertyContext",
             factory
        ]);
})(angular);

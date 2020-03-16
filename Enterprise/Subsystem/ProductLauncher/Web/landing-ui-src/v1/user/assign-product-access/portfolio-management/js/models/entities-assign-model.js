(function (angular, undefined) {
    "use strict";

    function factory(
        tabsContext,
        roleConfig
    ) {
        var model = {};

        model.init = function () {
            model.entityData = {};
            return model;
        };

        model.setEntityData = function () {
            model.entityData = tabsContext.get().data;
            return model;
        };

        model.setData = function (data) {
            model.data = data;
        };

        model.getData = function () {
            return model.data;
        };

        model.isReady = function () {
            return model.entityData.propertiesList.length > 0;
        };

        model.reset = function () {
            model.entityData = {};
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("pmEntitiesAssignModel", [
            "pmAssignEntitiesContext",
             factory
        ]);
})(angular);

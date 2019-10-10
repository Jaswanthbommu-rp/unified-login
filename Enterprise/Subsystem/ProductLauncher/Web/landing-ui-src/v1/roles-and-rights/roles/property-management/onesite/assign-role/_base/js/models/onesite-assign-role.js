(function(angular, undefined) {
    "use strict";

    function factory(

        gridModel,
        gridTransformSvc,
        gridPaginationModel,
        tabsContext,
        $filter,
        roleConfig
    ) {
        var model = {};

        var cfg = {
            recordsPerPage: 10
        };

        model.init = function() {
            model.roleData = {};
            model.assignRoleName = "";
            model.currentType = "";
            return model;
        };

        model.setRoleData = function() {
            model.roleData = tabsContext.get().data;            
            // model.currentType = "edit";
            // model.assignRoleName = angular.copy(model.roleData.name);
            // model.headerText = "Edit " + model.roleData.roletype +" Role";
            

            if (tabsContext.get().type === "edit") {
                model.currentType = "edit";
                model.assignRoleName = angular.copy(model.roleData.name);
                model.headerText = "Edit " + model.roleData.roletype +" Role";
                roleConfig.assignRoleName.readonly = model.roleData.roletype.toLowerCase() === "default" ? true : false;
            } else {
                model.headerText = "Assign " + model.roleData.roletype + " Role";
                roleConfig.assignRoleName.readonly = true;
            }
            return model;
        };

        model.getDataSvc = function(dataSvcGet) {
            var params = {};
            dataSvcGet.getData(params)
                .then(model.setDataFromSvc, model.setDataErr);
        };

        model.setData = function(data) {
            model.data = data;
        };

        model.getData = function() {
            return model.data;
        };

        model.setDataFromSvc = function(data) {
            model.setData(data);
        };

        model.setDataErr = function(data) {
            logc("Error = > ", data);
        };

        model.reset = function() {
            model.headerText = "";
            model.assignRoleName = "";
            model.currentType = "";
            model.roleData = {};
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("onesiteAssignRoleModel", [
            "rpGridModel",
            "rpGridTransform",
            "rpGridPaginationModel",
            "onesiteAssignTabsContext",
            "$filter",
            "onesiteAssignRoleFormConfig",
            factory
        ]);
})(angular);
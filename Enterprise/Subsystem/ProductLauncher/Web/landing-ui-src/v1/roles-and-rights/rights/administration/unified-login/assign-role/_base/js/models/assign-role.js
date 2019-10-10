(function(angular, undefined) {
    "use strict";

    function factory(
        gridConfig,
        gridModel,
        gridTransformSvc,
        gridPaginationModel,
        tabsContext,
        $filter
    ) {
        var model = {};

        var cfg = {
            recordsPerPage: 10
        };

        model.init = function() {
            model.roleData = {};
            return model;
        };

        model.gridInit = function() {
            var grid = gridModel(),
                gridTransform = gridTransformSvc(),
                gridPagination = gridPaginationModel();

            model.grid = grid;
            model.grid.busy(true);

            gridTransform.watch(grid);
            grid.setConfig(gridConfig);

            gridPagination.setConfig(cfg);
            gridPagination.setGrid(grid).trackSelection(gridConfig.getTrackSelectionConfig());

            model.gridPagination = gridPagination;
        };


        model.setRoleData = function() {
            model.roleData = tabsContext.get().data;
            return model;
        };

        model.getRoleData = function() {
            return model.roleData;
        };

        model.setData = function(data) {
            model.data = data;
        };


        model.getData = function() {
            return model.data;
        };

        model.setExistAssignedData = function(data) {
            model.existAssignedData = [];
            data.records.forEach(function(item) {
                if (item.isAssigned === true) {
                    model.existAssignedData.push(item.id);
                }
            });
        };

        model.getExistAssignedData = function() {
            return model.existAssignedData;
        };

        model.getAllAssignedData = function() {

            var allAssigned = [];
            model.getData().records.forEach(function(item) {
                if (item.isAssigned === true) {
                    allAssigned.push(item.id);
                }
            });

            return allAssigned;
        };

        model.getNewAssignedData = function() {
            var newAssigned = [];
            var allAssigned = model.getAllAssignedData();
            var extAssigned = model.getExistAssignedData();

            allAssigned.forEach(function(item) {

                var i = extAssigned.indexOf(item);
                if (i === -1) {
                    newAssigned.push(item);
                }


            });

            return newAssigned;
        };

        model.getUnAssignedData = function() {
            var unAssignedData = [];
            var allAssigned = model.getAllAssignedData();
            var extAssigned = model.getExistAssignedData();

            extAssigned.forEach(function(item) {

                var i = allAssigned.indexOf(item);
                if (i === -1) {
                    unAssignedData.push(item);
                }


            });
            return unAssignedData;
        };


        model.checkIsSelected = function() {

            var records = $filter("filter")(model.getData().records, {
                isAssigned: true
            });

            if (records != undefined && records.length > 0) {
                return true;
            }
            return false;
        };

        model.getSelectedRecords = function() {
            return model.grid.getSelectionChanges();
        };

        model.setGridPagination = function(data) {
            model.grid.busy(false);
            model.gridPagination.setData(data.records).goToPage({
                number: 0
            });
        };

        model.setDefaultTypeDisabled = function(data) {
            data.records.forEach(function(item) {
                angular.extend(item, { disableSelection: false });
                
                if (item.roletype.toLowerCase() === "system") {
                    item.disableSelection = true;
                }
            });
            return data;
        };

        model.setEditorWithNoRightDisabled = function(data) {              
                data.records.forEach(function(item) {
                    if (item.isEditorHasRight === false) {
                        angular.extend(item, { disableSelection: false });
                        item.disableSelection = true;
                    }
                });
           
            return data;
        };

        model.resetGridFilters = function() {
            model.grid.resetFilters();
        };


        model.setDataErr = function(data) {
            logc("Error: ", data);
        };

        model.reset = function() {
            model.roleData = {};
            model.grid.flushData();
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("umAssignRolesToRightsModel", [
            "umAssignRolesToRightsConfig",
            "rpGridModel",
            "rpGridTransform",
            "rpGridPaginationModel",
            "umAssignRolesToRightsContext",
            "$filter",
            factory
        ]);
})(angular);
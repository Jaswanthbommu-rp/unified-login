//  Assign Roles  Model

(function(angular, undefined) {
    "use strict";

    function factory(
        gridConfig,
        gridModel,
        gridTransformSvc,
        gridPaginationModel,
        tabsContext,
        $filter,
        products
    ) {
        var model = {},
            cfg = {
                recordsPerPage: 10
            };

        model.init = function() {

            return model;
        };

        model.gridInit = function() {
            var grid = gridModel(),
                gridTransform = gridTransformSvc(),
                gridPagination = gridPaginationModel();
            model.roleData = tabsContext.get();
            model.grid = grid;
            model.grid.busy(true);

            gridTransform.watch(grid);

            grid.setConfig(gridConfig);
            gridPagination.setConfig(cfg);
            gridPagination.setGrid(grid).trackSelection(gridConfig.getTrackSelectionConfig());

            model.gridPagination = gridPagination;
        };


        model.getData = function() {
            return model.data;
        };

        model.setData = function(data) {
            model.data = data;
        };

        model.setExistAssignedData = function(data) {
            model.existAssignedData = [];
            data.records.forEach(function(item) {
                if (item.assigned === true) {
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
                if (item.assigned === true) {
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
                assigned: true
            });

            if (records != undefined && records.length > 0) {
                return true;
            }
            return false;
        };

        model.getSelectedRecords = function() {
            return model.grid.getSelectionChanges();
        };

        model.setDataFromSvc = function(data) {
            model.setGridPagination(data);
        };

        model.setGridPagination = function(data) {
            model.grid.busy(false);
            model.gridPagination.setData(data.records).goToPage({
                number: 0
            });
        };


        model.resetGridFilters = function() {
            model.grid.resetFilters();
        };

        model.reset = function() {
            model.grid.destroy();
            model.gridPagination.destroy();
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("userMgmtAssignRightsModel", [
            "userMgmtAssignRightsConfig",
            "rpGridModel",
            "rpGridTransform",
            "rpGridPaginationModel",
            "userMgmtAssignTabsContext",
            "$filter",
            "userMgmtProductsData",
            factory
        ]);
})(angular);
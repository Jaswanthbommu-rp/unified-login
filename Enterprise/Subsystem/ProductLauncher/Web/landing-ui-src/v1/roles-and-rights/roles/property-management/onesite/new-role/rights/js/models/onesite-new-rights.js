//  new Roles  Model

(function(angular, undefined) {
    "use strict";

    function factory(
        gridConfig,
        gridModel,
        gridTransformSvc,
        gridPaginationModel,
        $filter
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

            model.grid = grid;
            model.grid.busy(true);
            gridTransform.watch(grid);
            grid.setConfig(gridConfig);

            gridPagination.setConfig(cfg);
            gridPagination.setGrid(grid).trackSelection(gridConfig.getTrackSelectionConfig());

            model.gridPagination = gridPagination;
        };



        model.setData = function(data) {
            model.data = data;
        };

        model.getData = function() {
            return model.data;
        };

        model.getNewAssignedData = function() {
            var newAssigned = [];

            var records = $filter("filter")(model.getData().records, {
                assigned: true
            });

            records.forEach(function(item) {
                newAssigned.push(item.id);
            });

            return newAssigned;
        };


        model.getSelectedRecords = function() {
            return model.grid.getSelectionChanges();
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


        model.setGridPagination = function(data) {
            model.grid.busy(false);
            model.gridPagination.setData(data.records).goToPage({
                number: 0
            });
        };

        model.setDataErr = function(data) {
            logc("Error = > ", data);
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
        .factory("onesiteNewRightsModel", [
            "onesiteNewRightsConfig",
            "rpGridModel",
            "rpGridTransform",
            "rpGridPaginationModel",
            "$filter",
            factory
        ]);
})(angular);
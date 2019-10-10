(function (angular, undefined) {
    "use strict";

    function factory(
        $filter,
        gridConfig,
        gridModel,
        gridTransformSvc,
        gridPaginationModel,
        persona,
        dataSvcGet
    ) {
        var model = {},
            cfg = {
                recordsPerPage: 10
            };

        model.init = function () {            
            model.searchUser = "";           
            return model;
        };

        model.initGrid = function () {
            var grid = gridModel(),
                gridTransform = gridTransformSvc(),
                gridPagination = gridPaginationModel();
            model.grid = grid;
            // model.grid.busy(true);
            gridTransform.watch(grid);
            grid.setConfig(gridConfig);

            gridPagination.setConfig(cfg);
            gridPagination.setGrid(grid).trackSelection(gridConfig.getTrackSelectionConfig());

            model.gridPagination = gridPagination;

           
            return model;
        };


        model.getDataSvc = function (filter) {
            if (persona.isReady()) {
                var params = {
                    editorPersonaId: persona.getId(),
                    filter: filter
                };
                dataSvcGet.get(params, model.setDataFromSvc, model.setDataErr);
            }
        };

        model.getSelectedRecords = function () {
            return model.grid.getSelectionChanges();
        };

        model.setData = function (data) {
            model.data = data;
        };

        model.getData = function () {
            return model.data;
        };

        model.setDataFromSvc = function (data) {    
            model.extendData(data);        
            model.setData(data);
            model.setFilteredDataToGrid(data.records);
        };

        model.extendData = function (data) {
            data.records.forEach(function (item) {
                angular.extend(item, {                    
                    name:  item.firstName + " " + item.lastName ,                    
                });
            });
        };

         model.flushBackupData = function () {
            model.gridPagination.flushBackupData();
        };


       
        model.setDataErr = function (data) {
            logc("Error = > ", data);
        };

        
         model.setBusyGrid = function () {
            model.grid.busy(true);
         };

        model.setFilteredDataToGrid = function (d) {
            var data = {
                records: d
            };
            model.gridPagination.flushBackupData();
            model.grid.busy(false);
            model.gridPagination.setData(data.records).goToPage({
                number: 0
            });
        };

        model.getFilteredData = function (inp) {
            var filObj = [];
            model.getData().records.forEach(function (item) {
                var i = item.filterData.toLowerCase().indexOf(inp);
                if (i !== -1) {
                    filObj.push(item);
                }
            });

            return filObj;
        };

        
       
        model.reset = function () {
            model.personaWatch();
            model.searchUser = "";
            model.grid.destroy();
            model.gridPagination.destroy();
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("empAccessUserModel", [
            "$filter",
            "empAccessUserGridConfig",
            "rpGridModel",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "empAccessUserSvc",
            factory
        ]);
})(angular);

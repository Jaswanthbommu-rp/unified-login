//  rights Model

(function(angular, undefined) {
    "use strict";

    function factory(
        $filter,
        gridConfig,
        gridModel,
        gridTransformSvc,
        gridActions,
        gridPaginationModel,
        pubsub,
        svc,
        assignRolesAside,
        tabsContext,
        user,
        $modal,
        productsMenu,
        persona,
        $timeout

    ) {
        var model = {},
            cfg = {
                recordsPerPage: 10
            };

        model.asideScope = {};

        model.init = function() {
            model.initWatch();
            return model;
        };

        model.initWatch = function() {
            model.assignUpdate = pubsub.subscribe("acctAssignRolesToRight.update", model.updateGrid);
            model.productChange = pubsub.subscribe("acctSettings.productChange", model.getDatabyProduct);
            return model;
        };

        model.initGrid = function() {

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
            model.getDataSvc();

            return model;

        };

        model.updateGrid = function() {
            model.grid.resetFilters();
            model.grid.busy(true);
            model.getDataSvc();            
        };

        // model.getDataSvc = function() {
        //     if(productsMenu.isReady){
        //         var selproduct = productsMenu.getSelProduct();
        //         var params = { editorPersonaId: persona.getId(), application : selproduct };
        //         svc.get(params, model.setDataFromSvc, model.setDataErr);
        //     }else{
        //         $timeout(model.getDataSvc(), 1000);
        //     }
        // };

        model.getDataSvc = function() {
            var params = { editorPersonaId: persona.getId() };
            svc.get(params, model.setDataFromSvc, model.setDataErr);
        };

        model.getSelectedRecords = function() {
            return model.grid.getSelectionChanges();
        };


        model.assignRolestoRights = function(record) {
            tabsContext.set({ type: "assign", data: record });
            assignRolesAside.show();
        };


        model.setRoles = function(data) {
            model.data = data.records;
        };

        model.getRoles = function() {
            return model.data;
        };

        model.setDataFromSvc = function(data) {            
            model.setRoles(data);
            var selproduct = productsMenu.getSelProduct();
            var d = model.getFilteredData(selproduct);
            model.setFilteredDataToGrid(d);
        };

        // model.getDatabyProduct = function() {
        //      model.grid.busy(true);
        //      model.getDataSvc();
        //     // var selproduct = productsMenu.getSelProduct();
        //     // var d = model.getFilteredData(selproduct);
        //     // model.setFilteredDataToGrid(d);
        // };
        model.getDatabyProduct = function() {
            model.grid.resetFilters();
            var selproduct = productsMenu.getSelProduct();
            var d = model.getFilteredData(selproduct);
            model.setFilteredDataToGrid(d);
        };

        model.setFilteredDataToGrid = function(d) {

            var data = {
                records: d
            };            
            model.gridPagination.flushBackupData();
            model.grid.busy(false);
            model.gridPagination.setData(data.records).goToPage({
                number: 0
            });
        };

        
        model.getFilteredData = function(val) {
            var filObj = $filter("filter")(model.getRoles(), {
                centerName: val
            },true);
            return filObj;
        };

        model.setDataErr = function(data) {
            logc("Error: ", data);
        };

        model.resetGridFilters = function() {
            model.grid.resetFilters();
        };

        model.reset = function() {            
            model.grid.destroy();
            model.gridPagination.destroy();
            model.assignUpdate();
            model.productChange();
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("rolAndRhtAcctRightsModel", [
            "$filter",
            "acctRightsGridConfig",
            "rpGridModel",
            "rpGridTransform",
            "acctRightsGridActions",
            "rpGridPaginationModel",
            "pubsub",
            "acctRightsSvc",
            "acctAssignRolesToRightsAside",
            "acctAssignRolesToRightsContext",
            "userSessionModel",
            "$modal",
            "acctProductsSelectMenu",
            "personaDetails",
            "$timeout",
            factory
        ]);
})(angular);
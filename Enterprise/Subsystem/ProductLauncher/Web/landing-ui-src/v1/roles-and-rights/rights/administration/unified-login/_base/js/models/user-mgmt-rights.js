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
        persona        

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
            model.assignUpdate = pubsub.subscribe("umAssignRolesToRight.update", model.updateGrid);
            
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

        model.getDataSvc = function() {
            var params = { 
                                            
                editorPersonaId: persona.getId(),
                partyId:  persona.data.organization.partyId };
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

            var d = model.getFilteredData();
            model.setFilteredDataToGrid(d);
        };

        model.getDatabyProduct = function() {                        
            var d = model.getFilteredData();
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

        model.getFilteredData = function() {            
            var filObj = model.getRoles();
            return filObj;
        };

        model.setDataErr = function(data) {
            logc("Error: ", data);
        };

        model.resetGridFilters = function() {
            model.grid.resetFilters();
        };

        model.reset = function() {
            // model.dataReq.$cancelRequest();
            model.grid.destroy();
            model.gridPagination.destroy();
            model.assignUpdate();            
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("userMgmtRightsModel", [
            "$filter",
            "userMgmtRightsGridConfig",
            "rpGridModel",
            "rpGridTransform",
            "userMgmtRightsGridActions",
            "rpGridPaginationModel",
            "pubsub",
            "userMgmtRightsSvc",
            "umAssignRolesToRightsAside",
            "umAssignRolesToRightsContext",
            "userSessionModel",
            "$modal",   
            "personaDetails",         
            factory
        ]);
})(angular);
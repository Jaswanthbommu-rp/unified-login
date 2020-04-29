//  UserMgmt Rights List Controller

(function(angular, undefined) {
    "use strict";

    function EntitiesListAsideCtrl($scope, aside, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, dataModel, sync, pmDataModel, $filter) {
        var vm = this,
            filteredRecords,
            asideGrid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel();

        vm.init = function() {
            vm.title = "Assigned Entities";
            vm.subtitle = persona.data.organization.name;
            vm.asideGrid = asideGrid;
            gridTransform.watch(asideGrid);
            asideGrid.setConfig(gridConfig);
            gridPagination.setGrid(asideGrid);
            $scope.gridPagination = gridPagination;

            vm.model = dataModel;

            gridPagination.setConfig({
                recordsPerPage: 10
            });

            vm.personaWatch = angular.noop;

            vm.readyWatch = $scope.$watch(vm.isReady, vm.setData);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.gridAllWatch = asideGrid.subscribe("selectAll", vm.selectAllProperties);
            vm.gridSelectionWatch = asideGrid.subscribe("selectChange", vm.selectionChange);
            vm.filterData = asideGrid.subscribe("filterBy", vm.filter.bind(vm));
            return vm;
        };
        vm.selectionChange = function (record) {
            if (record) {
                sync.selectedEntitySync();
            }
        };
        vm.setData = function () {
            gridPagination.setData(vm.model.data.propertiesList).goToPage({
                number: 0
            });
        };

        vm.filter = function(filterBy){
            vm.filteredRecords = $filter("filter")(vm.model.data.propertiesList, filterBy);
        };

        vm.update = function(){
            pmDataModel.setChanged();
            aside.hide();
        };

        vm.cancel = function() {
            aside.hide();
        };

        vm.selectAllProperties = function (val) {
            if(vm.filteredRecords !== undefined){
                pmDataModel.setAllPropertiesData(vm.filteredRecords, val);
            }
            else{
                pmDataModel.setAllPropertiesData(vm.model.data.propertiesList, val);
            }

            sync.selectedEntitySync();
        };

        vm.destroy = function() {
            vm.destWatch();
            vm.personaWatch();
            asideGrid.destroy();
            gridTransform.destroy();
            gridPagination.destroy();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }

            vm = undefined;
            $scope = undefined;
            asideGrid = undefined;
            gridTransform = undefined;
            gridPagination = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("EntitiesListAsideCtrl", [
            "$scope",
            "entitiesListAside",
            "rpGridModel",
            "portfolioManagementEntitiesAsideGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "pmEntitiesAssignModel",
            "pmSyncManager",
            "portfolioManagementDataModel",
            "$filter",
            EntitiesListAsideCtrl
        ]);
})(angular);

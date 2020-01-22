//  PropertyGroups Controller

(function (angular, undefined) {
    "use strict";

    function OnSitePropertyGroupsCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, dataModel, userDetailsModel, pubsub, security, sync) {
        var vm = this,
            filteredRecords,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.grid = grid;
            vm.allProperties = false;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            gridTransform.watch(grid);
            grid.setConfig(gridConfig);
            gridPagination.setGrid(grid);
            $scope.gridPagination = gridPagination;
            gridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }
            sync.setGroupSelectKey("isAssigned");
            vm.gridSelectionWatch = grid.subscribe("selectChange", vm.selectionChange);
            vm.updateGridWatch = pubsub.subscribe("onsite.updateGrids", vm.updateGrid);
            vm.updateAll = pubsub.subscribe("onsite.allProperties", vm.allPropertiesSelected);
            vm.gridSelectAllWatch = grid.subscribe("selectAll", vm.selectAllPropertyGroup);
            vm.filterData = grid.subscribe("filterBy", vm.filter.bind(vm));
        };

        vm.allPropertiesSelected = function (val) {
            vm.allProperties = val;
            if(!val){
            vm.grid.selectAll(false);
            vm.grid.updateSelected();
            }
            sync.allGroupToPropertySync();
        };

        vm.updateGrid = function () {
            vm.grid.updateSelected();
        };

        vm.filter = function(filterBy){
            vm.filteredRecords = $filter("filter")(vm.dataReq.records, filterBy);
        };

        vm.selectionAll = function () {
            sync.allGroupToPropertySync();
        };

        vm.selectionChange = function (record) {
            if (record) {
                sync.groupToPropertySync(record);
            }
        };
        vm.isActive = function () {
            return dataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && dataModel.isActive()) {
                grid.busy(true);
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId()
                };

                vm.activeWatch();
                vm.personaWatch();
                vm.dataReq = dataSvc.get(params, vm.setData);
            }
        };

        vm.setData = function (resp) {
            grid.busy(false);
            if (resp.records && resp.records.length > 0) {
                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    resp.records.forEach(function (item) {
                        angular.extend(item, {
                            disableSelection: false
                        });
                        item.disableSelection = true;
                    });
                }

                vm.gridData = resp.records;
                sync.setGroupList(resp.records);
                gridPagination.setData(resp.records).goToPage({
                    number: 0
                });
                dataModel.setPropertyGroups(resp.records);
            }
            if (resp.isError) {
                vm.isDataError = true;
                if (resp.errorReason !== "") {
                    vm.dataErrorReason = resp.errorReason;
                }
                else {
                    vm.dataErrorReason = genericDataErrorReason;
                }
            }
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageOnSiteProductAccess;
        };

        vm.selectAllPropertyGroup = function (val) {
            if(vm.filteredRecords !== undefined){
                dataModel.setAllPropertyGroupData(vm.filteredRecords, val);
            }
            else{
                dataModel.setAllPropertyGroupData(vm.dataReq.records, val);
            }
            sync.allGroupToPropertySync();
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.gridSelectionWatch();
            vm.updateGridWatch();
            vm.gridSelectAllWatch();
            vm.updateAll();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }
            grid.destroy();
            gridTransform.destroy();
            gridPagination.destroy();
            grid = undefined;
            gridTransform = undefined;
            gridPagination = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("OnSitePropertyGroupsCtrl", [
            "$scope",
            "$filter",
            "onSitePropertyGroupsSvc",
            "rpGridModel",
            "onSitePropertyGroupsGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "onSiteDataModel",
            "userDetailsModel",
            "pubsub",
            "routeSecurity",
            "osSyncManager",
            OnSitePropertyGroupsCtrl
        ]);
})(angular);

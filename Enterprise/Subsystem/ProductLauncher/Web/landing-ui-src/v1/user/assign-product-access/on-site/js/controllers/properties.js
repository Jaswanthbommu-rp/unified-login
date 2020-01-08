//  Properties Controller

(function (angular, undefined) {
    "use strict";

    function OnSitePropertiesCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, dataModel, userDetailsModel, sync, pubsub, switchConfig, security) {
        var vm = this,
            filteredRecords,
            grid = gridModel(),
            hasViewUserAccess,
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.grid = grid;
            vm.allProperties = false;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            vm.hasViewUserAccess = security.isAllowed("viewUser") || vm.isUserHasManageProductAccess();
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
            sync.setPropertySelectKey("isAssigned");
            vm.gridSelectionWatch = grid.subscribe("selectChange", vm.selectionChange);
            vm.gridSelectAllWatch = grid.subscribe("selectAll", vm.selectAllProperties);
            vm.updateGridWatch = pubsub.subscribe("onsite.updateGrids", vm.updateGrid);
            vm.filterData = grid.subscribe("filterBy", vm.filter.bind(vm));
        };

        vm.updateGrid = function () {
            vm.grid.updateSelected();
        };

        vm.selectionAll = function () {
            sync.allPropertyToGroupSync();
        };
        
        vm.filter = function(filterBy){
            vm.filteredRecords = $filter("filter")(vm.dataReq.records, filterBy);
        };

        vm.selectionChange = function (record) {
            if (record) {
                sync.propertyToGroupSync(record);
            }
        };

        vm.isActive = function () {
            return dataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && dataModel.isActive()) {
                vm.allPropSwitch = switchConfig({
                    onChange: vm.setAllProperties,
                    disabled: security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()
                });
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
                sync.setPropertyList(resp.records);

                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    resp.records.forEach(function (item) {
                        angular.extend(item, {
                            disableSelection: false
                        });
                        item.disableSelection = true;
                    });
                    vm.allProperties = false;
                    dataModel.setProperties(resp.records);
                }
                else {
                    if (resp.additional && resp.additional.allProperties) {
                        vm.allProperties = resp.additional.allProperties;
                        vm.setAllProperties(true);
                    }
                    else {
                        dataModel.setProperties(resp.records);
                    }
                }
                gridPagination.setData(resp.records).goToPage({
                    number: 0
                });
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

        vm.setAllProperties = function (val) {
            pubsub.publish("onsite.allProperties", val);
            if (val) {
                var allPropertiesArray = [];
                allPropertiesArray.push(-1);
                dataModel.setProperties(allPropertiesArray);
                sync.allPropertyToGroupSync();
            }
            else {
                dataModel.setProperties(vm.dataReq.records);
            }
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageOnSiteProductAccess;
        };

        vm.selectAllProperties = function (val) {
            if(vm.filteredRecords !== undefined){
                dataModel.setAllPropertiesData(vm.filteredRecords, val);
            }
            else{
                dataModel.setAllPropertiesData(vm.dataReq.records, val);
            }
            sync.allPropertyToGroupSync();
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.gridSelectionWatch();
            vm.gridSelectAllWatch();
            vm.updateGridWatch();
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
        .controller("OnSitePropertiesCtrl", [
            "$scope",
            "$filter",
            "onSitePropertiesSvc",
            "rpGridModel",
            "onSitePropertiesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "onSiteDataModel",
            "userDetailsModel",
            "osSyncManager",
            "pubsub",
            "rpSwitchConfig",
            "routeSecurity",
            OnSitePropertiesCtrl
        ]);
})(angular);
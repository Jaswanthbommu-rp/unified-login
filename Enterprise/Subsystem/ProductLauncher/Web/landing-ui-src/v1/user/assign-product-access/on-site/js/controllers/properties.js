//  Properties Controller

(function (angular, undefined) {
    "use strict";

    function OnSitePropertiesCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, dataModel, userDetailsModel, sync, pubsub, switchConfig, security) {
        var vm = this,
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
            vm.gridAllWatch = grid.subscribe("selectAll", vm.selectionAll);
            vm.gridSelectAllWatch = grid.subscribe("selectAll", vm.selectAllProperties);
            vm.updateGridWatch = pubsub.subscribe("onsite.updateGrids", vm.updateGrid);
        };

        vm.updateGrid = function () {
            vm.grid.updateSelected();
        };

        vm.selectionAll = function () {
            sync.allPropertyToGroupSync();
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

                //clear selections, if there is any
                vm.grid.selectAll(false);
                vm.grid.updateSelected();
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
            dataModel.setAllPropertiesData(vm.dataReq.records, val);
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.gridSelectionWatch();
            vm.gridAllWatch();
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
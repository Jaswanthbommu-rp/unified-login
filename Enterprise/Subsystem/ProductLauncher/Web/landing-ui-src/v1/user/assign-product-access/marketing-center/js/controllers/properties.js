//  Properties Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function MCPropertiesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, MCDataModel, userDetailsModel, security, switchConfig) {
        var vm = this,
            allProperties,
            filteredRecords,
            IsAssignedNewPropertyByDefault,
            propertiesGrid = gridModel(),
            propertiesGridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel();

        vm.init = function () {
            vm.propertiesGrid = propertiesGrid;
            vm.propertiesError = $filter("productPanelText")("panelError.generic");
            vm.allProperties = false;
            vm.isAssignedNewPropertyByDefault = false;
            propertiesGridTransform.watch(propertiesGrid);
            propertiesGrid.setConfig(gridConfig);
            gridPagination.setGrid(propertiesGrid);
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

            vm.gridAllWatch = propertiesGrid.subscribe("selectAll", vm.selectAllProperties);
            vm.filterData = propertiesGrid.subscribe("filterBy", vm.filter.bind(vm));
        };

        vm.isActive = function () {
            return MCDataModel.isActive();
        };
        vm.filter = function (filterBy) {
            vm.filteredRecords = $filter("filter")(vm.dataReq.records, filterBy);
        };
        vm.loadData = function () {
            if (persona.isReady() && MCDataModel.isActive()) {
                vm.allPropSwitch = switchConfig({
                    onChange: vm.setNewPropertyState,
                    disabled: security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()
                });
                propertiesGrid.busy(true);
                var params = {
                    editorPersonaId: persona.getId(),
                    userPersonaId: userDetailsModel.getPersonaId()
                };

                vm.activeWatch();
                vm.personaWatch();
                vm.dataReq = dataSvc.get(params, vm.setData);
            }
        };

        vm.setData = function (resp) {
            propertiesGrid.busy(false);
            if (resp.records && resp.records.length > 0) {
                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    resp.records.forEach(function (item) {
                        angular.extend(item, {
                            disableSelection: false
                        });
                        item.disableSelection = true;
                    });
                }

                gridPagination.setData(resp.records).goToPage({
                    number: 0
                });
                
                if (resp.additional && resp.additional.isAssignedNewPropertyByDefault) {
                    vm.setNewPropertyState(resp.additional.isAssignedNewPropertyByDefault);
                    vm.isAssignedNewPropertyByDefault = resp.additional.isAssignedNewPropertyByDefault;
                } else {
                    vm.setNewPropertyState(false);
                }
                MCDataModel.setProperties(resp.records);
            }
            if (resp.isError) {
                vm.isPropertiesError = true;
            }
        };

        vm.selectAllProperties = function (val) {
            if (vm.filteredRecords !== undefined) {
                MCDataModel.setAllProperties(vm.filteredRecords, val);
            }
            else {
                MCDataModel.setAllProperties(vm.dataReq.records, val);
            }
        };
        
        vm.setNewPropertyState = function (val) {
            if (val) {
                MCDataModel.setNewPropertyState(val);
            }
            else {
                MCDataModel.setNewPropertyState(false);
            }
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageMarketingCenterProductAccess;
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.gridAllWatch();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }
            propertiesGrid.destroy();
            propertiesGridTransform.destroy();
            gridPagination.destroy();
            propertiesGrid = undefined;
            propertiesGridTransform = undefined;
            gridPagination = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("MCPropertiesGridCtrl", [
            "$scope",
            "$filter",
            "MCPropertiesSvc",
            "rpGridModel",
            "MCPropertiesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "MarketingCenterDataModel",
            "userDetailsModel",
            "routeSecurity",
            "rpSwitchConfig",
            MCPropertiesGridCtrl
        ]);
})(angular);

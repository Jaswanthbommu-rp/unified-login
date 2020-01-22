//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function Lead2LeaseRolesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, lead2LeaseDataModel, userDetailsModel, menuConfig, presetModel, security) {
        var vm = this,
            filteredRecords,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.grid = grid;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            vm.presetRoles = [];
            vm.roleSelected = "";
            vm.roleSelect = menuConfig({
                nameKey: "name",
                valueKey: "id",
                fieldName: "roleSelect",
                onChange: vm.roleSelectedChange
            });
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

            vm.gridAllWatch = grid.subscribe("selectAll", vm.selectAllRoles);
            vm.filterData = grid.subscribe("filterBy", vm.filter.bind(vm));
        };

        vm.isActive = function () {
            return lead2LeaseDataModel.isActive();
        };
        vm.filter = function(filterBy){
            vm.filteredRecords = $filter("filter")(vm.dataReq.records, filterBy);
        };
        vm.loadData = function () {
            if (persona.isReady() && vm.isActive()) {
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

        vm.roleSelectedChange = function (val) {
            var roleSelected = vm.presetRoles.find(function (item) {
                return item.id === val;
            });
            presetModel.setData(roleSelected);

            // Clear any selections
            vm.grid.selectAll(false);
            vm.grid.updateSelected();

            // Select defaults based on role
            vm.rights.forEach(function (item) {
                item.isAssigned = presetModel.containsId(item.id);
            });
            vm.grid.updateSelected();
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

                vm.rights = resp.records;
                gridPagination.setData(vm.rights).goToPage({
                    number: 0
                });
                if (resp.additional && resp.additional.presets && resp.additional.presets.length > 0) {
                    vm.setPresetRoles(resp.additional.presets);
                }

                lead2LeaseDataModel.setRoles(vm.rights);
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

        vm.selectAllRoles = function(val){
            if(vm.filteredRecords !== undefined){
                lead2LeaseDataModel.setAllRoles(vm.filteredRecords, val);
            }
            else{
                lead2LeaseDataModel.setAllRoles(vm.dataReq.records, val);
            } 
        };

        vm.setPresetRoles = function (roles) {
            vm.presetRoles.push(presetModel.getData());
            roles.forEach(function (option) {
                vm.presetRoles.push(option);
            });
            vm.roleSelect.setOptions(vm.presetRoles);
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageLead2LeaseProductAccess;
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.gridAllWatch();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }
            presetModel.reset();
            grid.destroy();
            gridTransform.destroy();
            gridPagination.destroy();
            grid = undefined;
            gridTransform = undefined;
            gridPagination = undefined;
            vm = undefined;
            $scope = undefined;
            filteredRecords=undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("Lead2LeaseRolesGridCtrl", [
            "$scope",
            "$filter",
            "lead2LeaseRolesSvc",
            "rpGridModel",
            "lead2LeaseRolesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "lead2LeaseDataModel",
            "userDetailsModel",
            "rpFormSelectMenuConfig",
            "presetRoleModel",
            "routeSecurity",
            Lead2LeaseRolesGridCtrl
        ]);
})(angular);

//  Revenue Management Companies Controller

(function (angular, undefined) {
    "use strict";

    function RevenueManagementCompaniesCtrl($scope, $filter, companyRoleDataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, RMDataModel, userDetailsModel, pubsub, switchConfig, sync) {
        var vm = this,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.grid = grid;
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

            sync.setRoleSelectKey("isAssigned");
            vm.updateGridWatch = pubsub.subscribe("RM.updateGrids", vm.updateGrid);

        };

        vm.updateGrid = function () {
            vm.grid.updateSelected();
        };

        vm.isActive = function () {
            return RMDataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && RMDataModel.isActive() && !RMDataModel.isCompanyDataExists()) {
                grid.busy(true);
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId(),
                    productName: "PO"
                };

                vm.personaWatch();
                vm.dataReq = companyRoleDataSvc.get(params, vm.setData);
            }

        };

        vm.setData = function (resp) {
            grid.busy(false);
            if (resp.records && resp.records.length > 0) {
                sync.setRoleList(resp.records);
                gridPagination.setData(resp.records).goToPage({
                    number: 0
                });

                RMDataModel.setCompanies(resp.records);
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

        vm.isReady = function () {
            return RMDataModel.isReady();
        };

        vm.setChanged = function () {
            RMDataModel.setChanged();
        };


        vm.destroy = function () {
            vm.destWatch();
            vm.updateGridWatch();
            sync.reset();
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
        .controller("RevenueManagementCompaniesCtrl", [
            "$scope",
            "$filter",
            "AssetOptimizationRolesSvc",
            "rpGridModel",
            "revenueManagementCompanyGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "revenueManagementDataModel",
            "userDetailsModel",
            "pubsub",
            "rpSwitchConfig",
            "rmSyncManager",
            RevenueManagementCompaniesCtrl
        ]);
})(angular);

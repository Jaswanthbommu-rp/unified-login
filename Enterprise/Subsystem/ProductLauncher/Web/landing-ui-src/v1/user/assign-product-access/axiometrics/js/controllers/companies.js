//  Axiometrics Company roleController

(function (angular, undefined) {
    "use strict";

    function AxmCompaniesCtrl($scope, $filter, companyRoleDataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, AXMDataModel, userDetailsModel, pubsub, switchConfig, sync) {
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
            sync.setRoleSelectKey("isAssigned");
            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }

            vm.updateGridWatch = pubsub.subscribe("AXM.updateGrids", vm.updateGrid);
        };

        vm.updateGrid = function () {
            vm.grid.updateSelected();
        };

        vm.loadData = function () {
            if (persona.isReady() && AXMDataModel.isActive() && !AXMDataModel.isCompanyDataExists()) {
                grid.busy(true);
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId(),
                    productName: "AX"
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

                AXMDataModel.setCompanies(resp.records);
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

        vm.isActive = function () {
            return AXMDataModel.isActive();
        };

        vm.isReady = function () {
            return AXMDataModel.isReady();
        };

        vm.setChanged = function () {
            AXMDataModel.setChanged();
        };


        vm.destroy = function () {
            vm.destWatch();
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
        .controller("AxmCompaniesCtrl", [
            "$scope",
            "$filter",
            "AssetOptimizationRolesSvc",
            "rpGridModel",
            "axmCompanyGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "axmDataModel",
            "userDetailsModel",
            "pubsub",
            "rpSwitchConfig",
            "axmSyncManager",
            AxmCompaniesCtrl
        ]);
})(angular);

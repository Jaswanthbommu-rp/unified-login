//  Revenue Management Companies Controller

(function (angular, undefined) {
    "use strict";

    function RMCompanyPropertiesCtrl($scope, $filter, companyPropertyDataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, rmDataModel, userDetailsModel, pubsub, switchConfig, sync) {
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
                recordsPerPage: 10
            });
            sync.setPropertySelectKey("isAssigned");
            vm.personaWatch = angular.noop;
            vm.readyWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            //vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);
            vm.readyWatch = $scope.$watch(vm.isReady, vm.loadData);

            vm.updateGridWatch = pubsub.subscribe("RMP.updateGrids", vm.updateGrid);
        };

        vm.updateGrid = function (data) {
            //vm.grid.updateSelected();
            if (data && data.length > 0) {
                gridPagination.setData(data).goToPage({
                    number: 0
                });
            }
        };

        vm.isActive = function () {
            return rmDataModel.isActive();
        };

        vm.isReady = function () {
            return rmDataModel.isReady();
        };

        vm.loadData = function () {
            var propertyList = sync.getPropertyList();
            if (vm.isReady() && propertyList.length == 0) {
                grid.busy(true);
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId(),
                    productName: "PO"
                };

                //vm.personaWatch();
                vm.readyWatch();
                vm.dataReq = companyPropertyDataSvc.get(params, vm.setData);
            }

        };

        vm.setData = function (resp) {
            grid.busy(false);
            if (resp.records && resp.records.length > 0) {
                gridPagination.setData(resp.records).goToPage({
                    number: 0
                });

                sync.setOriginalPropertyList(resp.records);
                sync.setPropertyList(resp.records);
                rmDataModel.setProperties(sync.getPropertyList());
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

        vm.setChanged = function () {
            rmDataModel.setChanged();
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.updateGridWatch();
            vm.personaWatch();
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
        .controller("RMCompanyPropertiesCtrl", [
            "$scope",
            "$filter",
            "AssetOptimizationCompanyPropertyListSvc",
            "rpGridModel",
            "rmCompanyPropertyAssignedGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "revenueManagementDataModel",
            "userDetailsModel",
            "pubsub",
            "rpSwitchConfig",
            "rmSyncManager",
            RMCompanyPropertiesCtrl
        ]);
})(angular);

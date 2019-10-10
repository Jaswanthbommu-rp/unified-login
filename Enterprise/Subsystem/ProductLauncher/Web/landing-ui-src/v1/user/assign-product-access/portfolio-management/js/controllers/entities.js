//  Portfolio Management Entities Controller

(function (angular, undefined) {
    "use strict";

    function PortfolioManagementEntitiesCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, PMDataModel, userDetailsModel, pubsub, switchConfig, sync) {
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

            vm.updateGridWatch = pubsub.subscribe("PM.updateGrids", vm.updateGrid);

        };

        vm.updateGrid = function () {
            vm.grid.updateSelected();
        };


        vm.isActive = function () {
            return PMDataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && PMDataModel.isActive()) {
                var params = {
                    productType: "PortfolioManagement",
                    subjectPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId()
                };

                vm.activeWatch();
                vm.personaWatch();
                vm.dataReq = dataSvc.get(params, vm.setData);
            }

        };

        vm.setData = function (resp) {
            if (resp.records && resp.records.length > 0) {
                sync.setRoleList(resp.records);
                gridPagination.setData(resp.records).goToPage({
                    number: 0
                });

                PMDataModel.setEntities(resp.records);
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
            return PMDataModel.isReady();
        };

        vm.setChanged = function () {
            PMDataModel.setChanged();
        };


        vm.destroy = function () {
            vm.destWatch();
            vm.updateGridWatch();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }
            sync.reset();
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
        .controller("PortfolioManagementEntitiesCtrl", [
            "$scope",
            "$filter",
            "ProductPropertiesSvc",
            "rpGridModel",
            "portfolioManagementEntitiesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "portfolioManagementDataModel",
            "userDetailsModel",
            "pubsub",
            "rpSwitchConfig",
            "pmSyncManager",
            PortfolioManagementEntitiesCtrl
        ]);
})(angular);

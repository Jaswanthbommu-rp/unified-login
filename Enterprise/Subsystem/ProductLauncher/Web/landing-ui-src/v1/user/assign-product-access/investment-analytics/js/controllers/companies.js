//  Investment Analytics Markets Controller

(function (angular, undefined) {
    "use strict";

    function InvestmentAnalyticsCompaniesCtrl($scope, $filter, companyRoleDataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, IADataModel, userDetailsModel, pubsub, switchConfig, sync) {
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

            vm.updateGridWatch = pubsub.subscribe("IA.updateGrids", vm.updateGrid);

        };

        vm.updateGrid = function () {
            vm.grid.updateSelected();
        };

        vm.isActive = function () {
            return IADataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && IADataModel.isActive() && !IADataModel.isCompanyDataExists()) {
                grid.busy(true);
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId(),
                    productName: "MA"
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

                IADataModel.setCompanies(resp.records);
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
            return IADataModel.isReady();
        };

        vm.setChanged = function () {
            IADataModel.setChanged();
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
        .controller("InvestmentAnalyticsCompaniesCtrl", [
            "$scope",
            "$filter",
            "AssetOptimizationRolesSvc",
            "rpGridModel",
            "investmentAnalyticsCompanyGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "investmentAnalyticsDataModel",
            "userDetailsModel",
            "pubsub",
            "rpSwitchConfig",
            "iaSyncManager",
            InvestmentAnalyticsCompaniesCtrl
        ]);
})(angular);

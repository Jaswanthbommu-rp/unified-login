//  Performance Analytics Companies Controller

(function (angular, undefined) {
    "use strict";

    function PerformanceAnalyticsCompaniesCtrl($scope, $filter, companyRoleDataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, paDataModel, userDetailsModel, pubsub, switchConfig, sync, aoStatus, companyGridConfigFactory) {
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
            vm.bmroleWatch = angular.noop;

            vm.bmroleWatch = aoStatus.subscribeBM(vm.setBenchmarkReady);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);
            vm.bmReadyWatch = $scope.$watch(vm.isBMReady, vm.loadBMRoleData);

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }

            sync.setRoleSelectKey("isAssigned");
            vm.updateGridWatch = pubsub.subscribe("PA.updateGrids", vm.updateGrid);

        };

        vm.updateGrid = function () {
            vm.grid.updateSelected();
        };


        vm.isActive = function () {
            return paDataModel.isActive();
        };

        vm.isReady = function () {
            return paDataModel.isReady();
        };

        vm.isBMReady = function () {
            return paDataModel.isReady() && paDataModel.isBenchmarkReady();
        };

        vm.loadData = function () {
            if (persona.isReady() && vm.isActive() && !paDataModel.isCompanyDataExists()) {
                grid.busy(true);
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId(),
                    productName: "PA"
                };

                vm.personaWatch();
                vm.dataReq = companyRoleDataSvc.get(params, vm.setData);
            }

        };

        vm.loadBMRoleData = function (value) {
            if (value) {
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId(),
                    productName: "BM"
                };

                vm.dataBMReq = companyRoleDataSvc.get(params, vm.setBMData);
            }
        };

        vm.setData = function (resp) {
            grid.busy(false);
            if (resp.records && resp.records.length > 0) {
                paDataModel.setCompanies(resp.records);
                sync.setRoleList(resp.records);
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

        vm.setBMData = function (resp) {
            if (resp.records && resp.records.length > 0) {
                paDataModel.setCompanyBMRoles(resp.records);
                sync.setBMRoleList(resp.records);
                vm.setGridBenchmarkRoleColumn("Benchmarking Role", "bmrole");
            }
        };

        vm.setBenchmarkReady = function (value) {
            paDataModel.setBenchmarkReady(value);
        };

        vm.setGridBenchmarkRoleColumn = function (name, keycol) {
            var getKeys = gridConfig.get();
            var getHeaders = gridConfig.getHeaders();
            var getFilters = gridConfig.getFilters();
            var data = {};

            getHeaders[0].forEach(function (item) {
                if (item.key === "bmrole") {
                    item.text = name;
                }
            });

            data.getKeys = getKeys;
            data.getHeaders = getHeaders;
            data.getFilters = getFilters;

            gridConfig = companyGridConfigFactory(data);
            vm.grid.setConfig(gridConfig);
        };

        vm.setChanged = function () {
            paDataModel.setChanged();
        };


        vm.destroy = function () {
            vm.destWatch();
            vm.bmroleWatch();
            vm.bmReadyWatch();
            vm.updateGridWatch();
            sync.reset();

            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }

            if (vm.dataBMReq) {
                vm.dataBMReq.$cancelRequest();
            }

            paDataModel.reset();
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
        .controller("PerformanceAnalyticsCompaniesCtrl", [
            "$scope",
            "$filter",
            "AssetOptimizationRolesSvc",
            "rpGridModel",
            "performanceAnalyticsCompanyGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "performanceAnalyticsDataModel",
            "userDetailsModel",
            "pubsub",
            "rpSwitchConfig",
            "paSyncManager",
            "aOStatusModel",
            "companyGridConfigFactory",
            PerformanceAnalyticsCompaniesCtrl
        ]);
})(angular);

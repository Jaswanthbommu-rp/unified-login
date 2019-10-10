//  Axometrics Markets Controller

(function (angular, undefined) {
    "use strict";

    function AXMMarketsCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, AXMDataModel, userDetailsModel, pubsub, switchConfig) {
        var vm = this,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.grid = grid;
            vm.allProperties = false;
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

            vm.updateGridWatch = pubsub.subscribe("IAM.updateGrids", vm.updateGrid);
        };

        vm.updateGrid = function () {
            vm.grid.updateSelected();
        };

        vm.isActive = function () {
            return AXMDataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && AXMDataModel.isActive()) {
                grid.busy(true);
                var companyData = AXMDataModel.getCompanies();
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId(),
                    selectedCompanies: [0],
                    productName: "AX"
                };

                vm.activeWatch();
                vm.personaWatch();
                vm.dataReq = dataSvc.get(params, vm.setData);
            }
        };

        vm.setData = function (resp) {
            grid.busy(false);
            if (resp.records && resp.records.length > 0) {
                gridPagination.setData(resp.records).goToPage({
                    number: 0
                });

                AXMDataModel.setPropertyGroups(resp.records);
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
        .controller("AXMMarketsCtrl", [
            "$scope",
            "$filter",
            "AssetOptimizationPropertyGroupSvc",
            "rpGridModel",
            "axmMarketsGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "axmDataModel",
            "userDetailsModel",
            "pubsub",
            "rpSwitchConfig",
            AXMMarketsCtrl
        ]);
})(angular);

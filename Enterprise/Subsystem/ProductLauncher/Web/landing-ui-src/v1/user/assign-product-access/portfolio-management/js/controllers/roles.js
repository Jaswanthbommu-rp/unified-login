//  PortfolioManagementRoles Controller

(function (angular, undefined) {
    "use strict";

    function PortfolioManagementRolesCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, PMDataModel, userDetailsModel, pubsub, security) {
        var vm = this,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            vm.grid = grid;
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

            vm.updateGridWatch = pubsub.subscribe("PAM.updateGrids", vm.updateGrid);
        };

        vm.updateGrid = function () {
            vm.grid.updateSelected();
        };

        vm.isActive = function () {
            return PMDataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && vm.isActive()) {

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
                if (security.isAllowed("viewUser")) {
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

                PMDataModel.setRoles(resp.records);
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
        .controller("PortfolioManagementRolesCtrl", [
            "$scope",
            "$filter",
            "ProductRolesSvc",
            "rpGridModel",
            "portfolioManagementRolesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "portfolioManagementDataModel",
            "userDetailsModel",
            "pubsub",
            "routeSecurity",
            PortfolioManagementRolesCtrl
        ]);
})(angular);

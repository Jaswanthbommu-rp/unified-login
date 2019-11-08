//  BusinessIntelligence PropertyGroups Controller

(function (angular, undefined) {
    "use strict";

    function BusinessIntelligencePropertyGroupsCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, dataModel, userDetailsModel, pubsub, statusModel, security) {
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
            vm.aoStatusWatch = statusModel.subscribeBI(vm.loadData);
        };

        vm.allPropertiesSelected = function (val) {
            vm.allProperties = val;
            vm.grid.selectAll(false);
            vm.grid.updateSelected();
        };

        vm.updateGrid = function () {
            vm.grid.updateSelected();
        };

        vm.isActive = function () {
            return dataModel.isActive();
        };

        vm.loadData = function (record) {
            if (record && dataModel.isActive()) {
                grid.busy(true);
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId(),
                    selectedCompanies: [record],
                    productName: "BI"
                };

                vm.dataReq = dataSvc.get(params, vm.setData);
            }
        };

        vm.setData = function (resp) {
            grid.busy(false);
            if (resp.records && resp.records.length) {
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
                dataModel.setPropertyGroups(resp.records);
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

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageAssetOptimizationProductAccess;
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.aoStatusWatch();
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
        .controller("BusinessIntelligencePropertyGroupsCtrl", [
            "$scope",
            "$filter",
            "AssetOptimizationPropertyGroupSvc",
            "rpGridModel",
            "businessIntelligencePropertyGroupsGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "businessIntelligenceDataModel",
            "userDetailsModel",
            "pubsub",
            "aOStatusModel",
            "routeSecurity",
            BusinessIntelligencePropertyGroupsCtrl
        ]);
})(angular);

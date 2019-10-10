//  revenueManagement Markets Controller

(function (angular, undefined) {
    "use strict";

    function RMPropertyGroupsCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, rmDataModel, userDetailsModel, pubsub, switchConfig, sync, security) {
        var vm = this,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.grid = grid;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            vm.allProperties = false;
            gridTransform.watch(grid);
            grid.setConfig(gridConfig);
            gridPagination.setGrid(grid);
            $scope.gridPagination = gridPagination;
            gridPagination.setConfig({
                recordsPerPage: 25
            });

            sync.setGroupSelectKey("isAssigned");
            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.companyWatch = pubsub.subscribe("RMCR.CompanySelected", vm.loadCompanyGroupData);
            vm.companyGroupResetWatch = pubsub.subscribe("RMCR.CompanyGroupReset", vm.resetGroupData);
            vm.companyId = 0;

            vm.updateGridWatch = pubsub.subscribe("RMG.updateGrids", vm.updateGrid);
        };

        vm.updateGrid = function () {
            vm.grid.updateSelected();
        };

        vm.loadCompanyGroupData = function (record) {
            if (record) {
                var companyGrpData = sync.getComapnyGroupsData(record.companyId);
                if (!companyGrpData) {
                    if (record && record.companyId > 0 && record.isAssigned) {
                        grid.busy(true);
                        var params = {
                            userPersonaId: userDetailsModel.getPersonaId(),
                            editorPersonaId: persona.getId(),
                            selectedCompanies: [record.companyId],
                            productName: "PO"
                        };
                        vm.companyId = record.companyId;
                        vm.dataReq = dataSvc.get(params, vm.setData);
                    }
                }
                else {
                    if (companyGrpData.groups.length > 0) {
                        vm.resetGroupData(companyGrpData.groups);
                    }
                }
            }

        };


        vm.isActive = function () {
            return rmDataModel.isActive();
        };


        vm.resetGroupData = function (data) {
            gridPagination.setData(data).goToPage({
                number: 0
            });

            rmDataModel.setPropertyGroups(data);
        };

        vm.setData = function (resp) {
            grid.busy(false);
            if (resp.records  && resp.records.length > 0) {
                var grouplist = sync.setCompanyGroupList(vm.companyId, resp.records);
                var data = {
                    "records": []
                };

                grouplist.companyGroupList.forEach(function (group) {
                    data.records.push(group);
                });

                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    data.records.forEach(function (item) {
                        angular.extend(item, {
                            disableSelection: false
                        });
                        item.disableSelection = true;
                    });
                }

                gridPagination.setData(data.records).goToPage({
                    number: 0
                });

                rmDataModel.setPropertyGroups(data.records);
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
            vm.companyWatch();
            vm.companyGroupResetWatch();
            vm.personaWatch();
            //vm.gridAllWatch();
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
        .controller("RMPropertyGroupsCtrl", [
            "$scope",
            "$filter",
            "AssetOptimizationPropertyGroupSvc",
            "rpGridModel",
            "rmPropertyGroupsGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "revenueManagementDataModel",
            "userDetailsModel",
            "pubsub",
            "rpSwitchConfig",
            "rmSyncManager",
            "routeSecurity",
            RMPropertyGroupsCtrl
        ]);
})(angular);

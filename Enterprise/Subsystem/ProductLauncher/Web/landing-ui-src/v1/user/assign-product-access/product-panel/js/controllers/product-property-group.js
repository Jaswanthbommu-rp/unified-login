//  Property Groups Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProductPropertyGroupsGridCtrl($scope, $filter, dataSvc, gridModel, gridTransformSvc, gridPaginationModel, security, persona, syncMgr) {
        var vm = this,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel();

        vm.init = function () {
            vm.grid = grid;
            vm.propertyGroupsError = $filter("productPanelText")("panelError.generic");
            vm.config = syncMgr.getProductGridConfig($scope.$parent.productId, "PropertyGroup");
            gridTransform.watch(grid);
            grid.setConfig(vm.config);
            gridPagination.setGrid(grid);
            $scope.gridPagination = gridPagination;
            gridPagination.setConfig({
                recordsPerPage: 25
            });
            vm.loadData();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.loadData = function () {
            //vm.dataReq = dataSvc.get(vm.setData);
        };

        vm.setData = function (resp) {
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
            }
            if (resp.isError) {
                vm.isPropertyGroupsError = true;
            }
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageMarketingCenterProductAccess;
        };

        vm.destroy = function () {
            vm.destWatch();
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
        .controller("ProductPropertyGroupsGridCtrl", [
            "$scope",
            "$filter",
            "MCPropertyGroupSvc",
            "rpGridModel",
            "rpGridTransform",
            "rpGridPaginationModel",
            "routeSecurity",
            "personaDetails",
            "productDataSyncManager",
            ProductPropertyGroupsGridCtrl
        ]);
})(angular);

//  Property Groups Grid Tab Controller

(function(angular, undefined) {
    "use strict";

    function MCPropertyGroupsGridCtrl($scope, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel) {
        var vm = this,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel();

        vm.init = function() {
            vm.grid = grid;
            gridTransform.watch(grid);
            grid.setConfig(gridConfig);
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
            gridPagination.setData(resp.records).goToPage({
                number: 0
            });
        };

        vm.destroy = function() {
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
        .controller("MCPropertyGroupsGridCtrl", [
            "$scope",
            "MCPropertyGroupSvc",
            "rpGridModel",
            "MCPropertyGroupGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            MCPropertyGroupsGridCtrl
        ]);
})(angular);

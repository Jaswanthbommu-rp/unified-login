//  Accounting Entity Ground Aside Controller

(function (angular, undefined) {
    "use strict";

    function AEntityGroupAsideCtrl($scope, aside, dataSvc, gridModel, gridConfig, gridTransformSvc, detailsModel, gridPaginationModel) {
        var vm = this,
            asideGrid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel();

        vm.init = function () {
            vm.title = "Entity Group Details";
            vm.subtitle = detailsModel.getName();
            vm.asideGrid = asideGrid;
            gridTransform.watch(asideGrid);
            asideGrid.setConfig(gridConfig);
            gridPagination.setGrid(asideGrid);
            $scope.gridPagination = gridPagination;
            gridPagination.setConfig({
                recordsPerPage: 25
            });
            vm.loadData();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.loadData = function () {
            vm.dataReq = dataSvc.get(vm.setData);
        };

        vm.setData = function (resp) {
            gridPagination.setData(resp.records).goToPage({
                number: 0
            });
        };

        vm.cancel = function () {
            aside.hide();
        };

        vm.destroy = function () {
            vm.destWatch();
            asideGrid.destroy();
            gridTransform.destroy();
            gridPagination.destroy();
            asideGrid = undefined;
            gridTransform = undefined;
            gridPagination = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("AEntityGroupAsideCtrl", [
            "$scope",
            "AEntityGroupDetailsAside",
            "AEntityGroupAsideSvc",
            "rpGridModel",
            "AEntityGroupAsideGridConfig",
            "rpGridTransform",
            "AEntityGroupDetailsModel",
            "rpGridPaginationModel",
            AEntityGroupAsideCtrl
        ]);
})(angular);

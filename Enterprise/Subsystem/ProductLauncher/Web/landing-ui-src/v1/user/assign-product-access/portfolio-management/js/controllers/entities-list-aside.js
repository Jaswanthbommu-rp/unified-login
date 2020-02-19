//  UserMgmt Rights List Controller

(function(angular, undefined) {
    "use strict";

    function EntitiesListAsideCtrl($scope, aside, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, dataModel) {
        var vm = this,
            asideGrid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel();

        vm.init = function() {
            vm.title = "Assigned Entities";
            vm.subtitle = persona.data.organization.name;
            vm.asideGrid = asideGrid;
            gridTransform.watch(asideGrid);
            asideGrid.setConfig(gridConfig);
            gridPagination.setGrid(asideGrid);
            $scope.gridPagination = gridPagination;

            vm.model = dataModel;

            gridPagination.setConfig({
                recordsPerPage: 10
            });

            vm.personaWatch = angular.noop;

            vm.readyWatch = $scope.$watch(vm.isReady, vm.setData);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            return vm;
        };

        vm.setData = function () {
            gridPagination.setData(dataModel.getData().propertiesList).goToPage({
                number: 0
            });
        };

        vm.update = function(){
            logc('update called!');
        };

        vm.cancel = function() {
            aside.hide();
        };

        vm.destroy = function() {
            vm.destWatch();
            vm.personaWatch();
            asideGrid.destroy();
            gridTransform.destroy();
            gridPagination.destroy();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }

            vm = undefined;
            $scope = undefined;
            asideGrid = undefined;
            gridTransform = undefined;
            gridPagination = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("EntitiesListAsideCtrl", [
            "$scope",
            "entitiesListAside",
            "rpGridModel",
            "portfolioManagementEntitiesAsideGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "pmEntitiesAssignModel",
            EntitiesListAsideCtrl
        ]);
})(angular);

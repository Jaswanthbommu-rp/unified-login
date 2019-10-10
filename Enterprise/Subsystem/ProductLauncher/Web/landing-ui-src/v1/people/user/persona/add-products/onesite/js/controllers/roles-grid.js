//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function OSRolesGridCtrl($scope, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, OSDataModel) {
        var vm = this,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel();

        vm.init = function () {
            vm.grid = grid;
            gridTransform.watch(grid);
            grid.setConfig(gridConfig);
            gridPagination.setGrid(grid);
            $scope.gridPagination = gridPagination;
            gridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.personaWatch = angular.noop;

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }

            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.loadData = function () {
            var params = {
                editorPersonaId: persona.getId(),
                userPersonaId: "0" // TODO: on edit replace with actual edited user's persona ID
            };

            vm.dataReq = dataSvc.get(params, vm.setData);
        };

        vm.setData = function (resp) {
            gridPagination.setData(resp.records).goToPage({
                number: 0
            });
            OSDataModel.setRoles(resp.records);
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.personaWatch();
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
        .controller("OSRolesGridCtrl", [
            "$scope",
            "OSRolesSvc",
            "rpGridModel",
            "osRolesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "OnesiteDataModel",
            OSRolesGridCtrl
        ]);
})(angular);

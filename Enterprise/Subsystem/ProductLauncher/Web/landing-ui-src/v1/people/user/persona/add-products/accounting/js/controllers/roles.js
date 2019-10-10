//  Roles Grid Tab Controller

(function(angular, undefined) {
    "use strict";

    function ARolesGridCtrl($scope, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, ADataModel) {
        var vm = this,
            rolesGrid = gridModel(),
            rolesGridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel();

        vm.init = function() {
            vm.rolesGrid = rolesGrid;
            rolesGridTransform.watch(rolesGrid);
            rolesGrid.setConfig(gridConfig);
            gridPagination.setGrid(rolesGrid);
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
                userPersonaId: "0"  // TODO: on edit replace with actual edited user's persona ID
            };
            vm.dataReq = dataSvc.get(params, vm.setData);
        };

        vm.setData = function (resp) {
            gridPagination.setData(resp.records).goToPage({
                number: 0
            });
            ADataModel.setRoles(resp.records);
        };

        vm.destroy = function() {
            vm.destWatch();
            vm.personaWatch();
            vm.dataReq.$cancelRequest();
            rolesGrid.destroy();
            rolesGridTransform.destroy();
            gridPagination.destroy();
            rolesGrid = undefined;
            rolesGridTransform = undefined;
            gridPagination = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ARolesGridCtrl", [
            "$scope",
            "ARolesSvc",
            "rpGridModel",
            "ARolesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "AccountingDataModel",
            ARolesGridCtrl
        ]);
})(angular);

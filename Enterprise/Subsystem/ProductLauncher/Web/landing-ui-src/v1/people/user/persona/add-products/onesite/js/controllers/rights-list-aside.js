//  Rights List Controller

(function (angular, undefined) {
    "use strict";

    function OSRightsListAsideCtrl($scope, aside, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, rightsModel, userModel, persona) {
        var vm = this,
            asideGrid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel();

        vm.init = function () {
            vm.title = "Role Detail";
            vm.subtitle = rightsModel.getName();
            vm.asideGrid = asideGrid;
            gridTransform.watch(asideGrid);
            asideGrid.setConfig(gridConfig);
            gridPagination.setGrid(asideGrid);
            $scope.gridPagination = gridPagination;

            gridPagination.setConfig({
                recordsPerPage: 10
            });

            vm.personaWatch = angular.noop;

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }

            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            return vm;
        };

        vm.loadData = function () {
            var params = {
                editorPersonaId: persona.getId(),
                roleId: rightsModel.getRoleID()
            };

            vm.dataReq = dataSvc.get(params, vm.setData);
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
            vm.personaWatch();
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
        .controller("OSRightsListAsideCtrl", [
            "$scope",
            "osRightsListAside",
            "OSRightsListAsideSvc",
            "rpGridModel",
            "OSRightsAsideGrigConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "onesiteRightsModel",
            "userSessionModel",
            "personaDetails",
            OSRightsListAsideCtrl
        ]);
})(angular);

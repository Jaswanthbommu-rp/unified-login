//  revenueManagement Group Property List Controller

(function (angular, undefined) {
    "use strict";

    function RMPropertyListAsideCtrl($scope, aside, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, propertiessModel, userModel, persona) {
        var vm = this,
            asideGrid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            userLoginName = "";

        vm.init = function () {
            vm.title = "Property Group Detail";
            vm.subtitle = propertiessModel.getName();
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
            asideGrid.busy(true);
            var params = {
                editorPersonaId: persona.getId(),
                userPersonaId: userModel.getPersonaId(),
                propertyGroupId: propertiessModel.getPropertyGroupID(),
                userLoginName: userModel.getLoginName() === undefined ? userLoginName : userModel.getLoginName()
            };

            vm.dataReq = dataSvc.get(params, vm.setData);
        };

        vm.setData = function (resp) {
            asideGrid.busy(false);
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
        .controller("RMPropertyListAsideCtrl", [
            "$scope",
            "rmPropertiesListAside",
            "AssetOptimizationGroupPropertyListSvc",
            "rpGridModel",
            "rmPropertiesAsideGrigConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "rmPropertiesModel",
            "userDetailsModel",
            "personaDetails",
            RMPropertyListAsideCtrl
        ]);
})(angular);

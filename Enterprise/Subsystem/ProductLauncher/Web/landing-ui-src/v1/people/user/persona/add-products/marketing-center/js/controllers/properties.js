//  Properties Grid Tab Controller

(function(angular, undefined) {
    "use strict";

    function MCPropertiesGridCtrl($scope, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, MCDataModel) {
        var vm = this,
            propertiesGrid = gridModel(),
            propertiesGridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel();

        vm.init = function() {
            vm.propertiesGrid = propertiesGrid;
            propertiesGridTransform.watch(propertiesGrid);
            propertiesGrid.setConfig(gridConfig);
            gridPagination.setGrid(propertiesGrid);
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
            MCDataModel.setProperties(resp.records);
        };

        vm.destroy = function() {
            vm.destWatch();
            vm.personaWatch();
            vm.dataReq.$cancelRequest();
            propertiesGrid.destroy();
            propertiesGridTransform.destroy();
            gridPagination.destroy();
            propertiesGrid = undefined;
            propertiesGridTransform = undefined;
            gridPagination = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("MCPropertiesGridCtrl", [
            "$scope",
            "MCPropertiesSvc",
            "rpGridModel",
            "MCPropertiesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "MarketingCenterDataModel",
            MCPropertiesGridCtrl
        ]);
})(angular);

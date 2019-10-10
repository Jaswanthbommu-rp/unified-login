//  Properties Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function OSPropertiesGridCtrl($scope, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, OSDataModel, switchConfig) {
        var vm = this,
            allProperties,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel();

        vm.init = function () {
            vm.grid = grid;
            vm.allProperties = false;
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
            vm.allPropSwitch = switchConfig({
                onChange: vm.setAllProperties
            });

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
            OSDataModel.setProperties(resp.records);
        };

        vm.setAllProperties = function(val) {
            if (val) {
                var allPropertiesArray = [];
                allPropertiesArray.push("all");
                OSDataModel.setProperties(allPropertiesArray);

                //clear selections, if theres any
                vm.dataReq.records.forEach(function (property) {
                    if (property.isAssigned) {
                        property.isAssigned = false;
                    }
                });
            }
            else {
                OSDataModel.setProperties(vm.dataReq);
            }
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
        .controller("OSPropertiesGridCtrl", [
            "$scope",
            "OSPropertiesSvc",
            "rpGridModel",
            "osPropertiesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "OnesiteDataModel",
            "rpSwitchConfig",
            OSPropertiesGridCtrl
        ]);
})(angular);

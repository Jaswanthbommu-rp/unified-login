//  Entities Grid Tab Controller

(function(angular, undefined) {
    "use strict";

    function AEntitiesGridCtrl($scope, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, ADataModel, switchConfig) {
        var vm = this,
            entitiesGrid = gridModel(),
            entitiesGridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel();

        vm.init = function() {
            vm.allProperties = false;
            vm.entitiesGrid = entitiesGrid;
            entitiesGridTransform.watch(entitiesGrid);
            entitiesGrid.setConfig(gridConfig);
            gridPagination.setGrid(entitiesGrid);
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
                userPersonaId: "0"  // TODO: on edit replace with actual edited user's persona ID
            };
            vm.dataReq = dataSvc.get(params, vm.setData);
        };

        vm.setData = function (resp) {
            gridPagination.setData(resp.records).goToPage({
                number: 0
            });
            ADataModel.setProperties(resp.records);
        };

        vm.setAllProperties = function(val) {
            if (val) {
                var allPropertiesArray = [];
                allPropertiesArray.push("all");
                ADataModel.setProperties(allPropertiesArray);

                //clear selections, if theres any
                vm.dataReq.records.forEach(function (property) {
                    if (property.isAssigned) {
                        property.isAssigned = false;
                    }
                });
            }
            else {
                ADataModel.setProperties(vm.dataReq);
            }
        };

        vm.destroy = function() {
            vm.destWatch();
            vm.personaWatch();
            vm.dataReq.$cancelRequest();
            entitiesGrid.destroy();
            entitiesGridTransform.destroy();
            gridPagination.destroy();
            entitiesGrid = undefined;
            entitiesGridTransform = undefined;
            gridPagination = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("AEntitiesGridCtrl", [
            "$scope",
            "AEntitiesSvc",
            "rpGridModel",
            "AEntitiesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "AccountingDataModel",
            "rpSwitchConfig",
            AEntitiesGridCtrl
        ]);
})(angular);

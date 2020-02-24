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

            vm.model = Object.assign({}, dataModel);

            gridPagination.setConfig({
                recordsPerPage: 10
            });

            vm.personaWatch = angular.noop;

            vm.readyWatch = $scope.$watch(vm.isReady, vm.setData);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.gridAllWatch = asideGrid.subscribe("selectAll", vm.selectAllProperties);
            return vm;
        };

        vm.setData = function () {
            gridPagination.setData(vm.model.data.propertiesList).goToPage({
                number: 0
            });
        };

        vm.update = function(){
            var assignedPropertiesCount = vm.model.data.propertiesList.filter(function (item) {
                return item.isAssigned === true;
            });
            vm.model.data.assignedProperties = assignedPropertiesCount.length+" of "+vm.model.data.propertiesList.length;
            aside.hide();
            logc('vm model');
            logc(vm.model.data);
            logc('datamodel');
            logc(dataModel);
        };

        vm.cancel = function() {
            logc('vm model');
            logc(vm.model.data);
            logc('datamodel');
            logc(dataModel);
            aside.hide();
        };

        vm.selectAllProperties = function (val) {
            vm.model.data.propertiesList.forEach(function (item) {
               item["isAssigned"] = val;
            });
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

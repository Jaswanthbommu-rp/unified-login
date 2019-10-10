//  Property Group Grid Tab Controller

(function(angular, undefined) {
    "use strict";

    function SMPropertyGroupGridCtrl($scope, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, pubsub, persona, SMDataModel) {
        var vm = this,
            propertyGroupGrid = gridModel(),
            propertyGroupGridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel();

        vm.init = function() {
            vm.propertyGroupGrid = propertyGroupGrid;
            propertyGroupGridTransform.watch(propertyGroupGrid);
            propertyGroupGrid.setConfig(gridConfig);
            gridPagination.setGrid(propertyGroupGrid);
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

            pubsub.subscribe("cc.property-group-radio", vm.updateRecords);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.loadData = function () {
            var params = {
                editorPersonaId: persona.getId(),
                userPersonaId: "0" // TODO: on edit replace with actual edited user's persona ID
            };
            vm.dataReq = dataSvc.get(params, vm.setData);
        };

        vm.updateRecords= function (record) {
            vm.dataReq.records.forEach(function (item) {
                item.isAssigned = item.id == record.id;
            });
        };

        vm.setData = function (resp) {
            gridPagination.setData(resp.records).goToPage({
                number: 0
            });
            SMDataModel.setProperties(resp.records);
        };

        vm.destroy = function() {
            vm.destWatch();
            vm.personaWatch();
            propertyGroupGrid.destroy();
            propertyGroupGridTransform.destroy();
            gridPagination.destroy();
            propertyGroupGrid = undefined;
            propertyGroupGridTransform = undefined;
            gridPagination = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("SMPropertyGroupGridCtrl", [
            "$scope",
            "SMPropertyGroupSvc",
            "rpGridModel",
            "SMPropertyGroupGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "SpendManagementDataModel",
            SMPropertyGroupGridCtrl
        ]);
})(angular);

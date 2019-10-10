//  Properties Grid Tab Controller

(function(angular, undefined) {
    "use strict";

    function VendCompPropertiesGridCtrl($scope, dataSvc, dataGroupSvc, gridModel, gridConfig, gridGroupConfig, gridTransformSvc, gridPaginationModel, pubsub, persona, VendCompDataModel) {
        var vm = this,
            propertiesGrid = gridModel(),
            propertyGroupGrid = gridModel(),
            propertyGroupGridTransform = gridTransformSvc(),
            propertiesGridTransform = gridTransformSvc(),
            propertiesGridPagination = gridPaginationModel(),
            propertyGroupGridPagination = gridPaginationModel();

        vm.init = function() {
            vm.propertySelect = "property";
            vm.propertiesGrid = propertiesGrid;
            propertiesGridTransform.watch(propertiesGrid);
            propertiesGrid.setConfig(gridConfig);
            propertiesGridPagination.setGrid(propertiesGrid);
            $scope.propertiesGridPagination = propertiesGridPagination;
            propertiesGridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.propertyGroupGrid = propertyGroupGrid;
            propertyGroupGridTransform.watch(propertyGroupGrid);
            propertyGroupGrid.setConfig(gridGroupConfig);
            propertyGroupGridPagination.setGrid(propertyGroupGrid);
            $scope.propertyGroupGridPagination = propertyGroupGridPagination;
            propertyGroupGridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.personaWatch = angular.noop;

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }

            pubsub.subscribe("vc.property-group-radio", vm.updateGroupRecords);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.loadData = function () {
            var params = {
                editorPersonaId: persona.getId(),
                userPersonaId: "0" // TODO: on edit replace with actual edited user's persona ID
            };

            vm.dataReq = dataSvc.get(params, vm.setData);
            vm.dataGroupReq = dataGroupSvc.get(params, vm.setGroupData);
        };

        vm.updateRecords= function (record) {
            vm.dataReq.records.forEach(function (item) {
                item.isAssigned = item.id == record.id;
            });
        };

        vm.updateGroupRecords= function (record) {
            vm.dataGroupReq.records.forEach(function (item) {
                item.isAssigned = item.id == record.id;
            });
        };

        vm.setGroupData = function (resp) {
            propertyGroupGridPagination.setData(resp.records).goToPage({
                number: 0
            });
            VendCompDataModel.setPropertyGroups(resp.records);
        };

        vm.setData = function (resp) {
            propertiesGridPagination.setData(resp.records).goToPage({
                number: 0
            });
            VendCompDataModel.setProperties(resp.records);
        };

        vm.resetDataModel = function () {
            if(vm.propertySelect === 'property') {
                vm.clearPropertyGroups();
            } else if(vm.propertySelect === 'group') {
                vm.clearProperties();
            } else {
                vm.clearProperties();
                vm.clearPropertyGroups();
            }
            vm.setAllProperties();
        };

        vm.clearProperties = function () {
            vm.dataReq.records.forEach(function (property) {
                if (property.isAssigned) {
                    property.isAssigned = false;
                }
            });
        };

        vm.clearPropertyGroups = function () {
            vm.dataGroupReq.records.forEach(function (propertyGroup) {
                if (propertyGroup.isAssigned) {
                    propertyGroup.isAssigned = false;
                }
            });
        };

        vm.setAllProperties = function() {
            if(vm.propertySelect === 'all') {
                var allPropertiesArray = [];
                allPropertiesArray.push(-1);
                VendCompDataModel.setProperties(allPropertiesArray);
            } else {
                VendCompDataModel.setProperties(vm.dataReq.records);
            }
        };

        vm.destroy = function() {
            vm.destWatch();
            vm.personaWatch();
            propertiesGrid.destroy();
            propertiesGridTransform.destroy();
            propertiesGridPagination.destroy();
            propertiesGrid = undefined;
            propertiesGridTransform = undefined;
            propertiesGridPagination = undefined;
            propertyGroupGrid.destroy();
            propertyGroupGridTransform.destroy();
            propertyGroupGridPagination.destroy();
            propertyGroupGrid = undefined;
            propertyGroupGridTransform = undefined;
            propertyGroupGridPagination = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("VendCompPropertiesGridCtrl", [
            "$scope",
            "VendCompPropertiesSvc",
            "VendCompPropertyGroupSvc",
            "rpGridModel",
            "VendCompPropertiesGridConfig",
            "VendCompPropertyGroupGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "VendorComplianceDataModel",
            VendCompPropertiesGridCtrl
        ]);
})(angular);

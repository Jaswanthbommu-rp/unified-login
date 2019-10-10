//  performance Analytics Assigned Property List Controller

(function (angular, undefined) {
    "use strict";

    function RMPropertyAssignedListAsideCtrl($scope, aside, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, propertiesModel, userModel, persona, dataModel, sync, rmDataModel, pubsub, security) {
        var vm = this,
            asideGrid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel();

        vm.init = function () {
            vm.title = "Assign Properties";
            vm.subtitle = "";
            vm.asideGrid = asideGrid;
            gridTransform.watch(asideGrid);
            asideGrid.setConfig(gridConfig);
            gridPagination.setGrid(asideGrid);
            $scope.gridPagination = gridPagination;

            vm.model = dataModel;
            vm.model.setPropertyData();
            vm.companyId = 0;
            vm.originalPropertyList = [];
            gridPagination.setConfig({
                recordsPerPage: 10
            });

            vm.personaWatch = angular.noop;
            vm.readyWatch = $scope.$watch(vm.isReady, vm.setData);
            vm.gridSelectionWatch = asideGrid.subscribe("selectChange", vm.selectionChange);
            vm.gridAllWatch = asideGrid.subscribe("selectAll", vm.selectionAll);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            return vm;
        };

        vm.isReady = function () {
            return dataModel.isReady();
        };

        vm.selectionAll = function (bool) {
            var data = sync.allPropertiesSync(vm.companyId, bool);
            rmDataModel.setSelectedProperties(data.propertyList);
        };

        vm.selectionChange = function (record) {
            if (record) {
                var data = sync.selectedPropertySync(record);
                rmDataModel.setSelectedProperties(data.propertyList);
            }
        };

        vm.setData = function () {
            if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                dataModel.propertyData.properties.forEach(function (item) {
                    angular.extend(item, {
                        disableSelection: false
                    });
                    item.disableSelection = true;
                });
            }

            gridPagination.setData(dataModel.propertyData.properties).goToPage({
                number: 0
            });

            vm.subtitle = dataModel.propertyData.companyName;
            vm.companyId = dataModel.propertyData.companyId;
        };

        vm.cancel = function () {
            aside.hide();
        };

        vm.update = function () {
            var properties = rmDataModel.getSelectedProperties();
            rmDataModel.setProperties(properties);
            rmDataModel.setChanged();
            pubsub.publish("RMP.updateGrids", sync.getPropertyList());
            aside.hide();
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageAssetOptimizationProductAccess;
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.personaWatch();
            vm.readyWatch();
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
        .controller("RMPropertyAssignedListAsideCtrl", [
            "$scope",
            "rmAssignPropertyAside",
            "AssetOptimizationGroupPropertyListSvc",
            "rpGridModel",
            "rmCompanyPropertyGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "rmPropertiesModel",
            "userDetailsModel",
            "personaDetails",
            "rmPropertyAssignModel",
            "rmSyncManager",
            "revenueManagementDataModel",
            "pubsub",
            "routeSecurity",
            RMPropertyAssignedListAsideCtrl
        ]);
})(angular);

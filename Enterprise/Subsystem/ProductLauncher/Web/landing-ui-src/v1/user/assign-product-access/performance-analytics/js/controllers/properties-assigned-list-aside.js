//  performance Analytics Assigned Property List Controller

(function (angular, undefined) {
    "use strict";

    function PAPropertyAssignedListAsideCtrl($scope, $filter, aside, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, propertiesModel, userModel, persona, dataModel, sync, paDataModel, pubsub, security) {
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

            gridPagination.setConfig({
                recordsPerPage: 10
            });
            vm.filterBy = undefined;
            vm.personaWatch = angular.noop;
            vm.readyWatch = $scope.$watch(vm.isReady, vm.setData);
            vm.gridSelectionWatch = asideGrid.subscribe("selectChange", vm.selectionChange);
            vm.gridAllWatch = asideGrid.subscribe("selectAll", vm.selectionAll);
            vm.filterData = asideGrid.subscribe("filterBy", vm.filter.bind(vm));
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            return vm;
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageAssetOptimizationProductAccess;
        };

        vm.isReady = function () {
            return dataModel.isReady();
        };

        vm.selectionAll = function (bool) {
            var data = sync.allPropertiesSync(vm.companyId, bool, vm.filterBy);
            paDataModel.setSelectedProperties(data.propertyList);
        };

        vm.filter = function(filterBy){
            vm.filterBy = filterBy;
        };

        vm.selectionChange = function (record) {
            if (record) {
                var data = sync.selectedPropertySync(record);
                paDataModel.setSelectedProperties(data.propertyList);
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
            //pubsub.publish("PAP.updateGrids", sync.getOriginalPropertyList());
            aside.hide();
        };

        vm.update = function () {
            var properties = paDataModel.getSelectedProperties();
            paDataModel.setProperties(properties);
            paDataModel.setChanged();
            pubsub.publish("PAP.updateGrids", sync.getPropertyList());
            aside.hide();
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.personaWatch();
            vm.readyWatch();
            vm.gridAllWatch();
            vm.gridSelectionWatch();
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
        .controller("PAPropertyAssignedListAsideCtrl", [
            "$scope",
            "$filter",
            "paAssignPropertyAside",
            "AssetOptimizationGroupPropertyListSvc",
            "rpGridModel",
            "paCompanyPropertyGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "paPropertiesModel",
            "userDetailsModel",
            "personaDetails",
            "paPropertyAssignModel",
            "paSyncManager",
            "performanceAnalyticsDataModel",
            "pubsub",
            "routeSecurity",
            PAPropertyAssignedListAsideCtrl
        ]);
})(angular);

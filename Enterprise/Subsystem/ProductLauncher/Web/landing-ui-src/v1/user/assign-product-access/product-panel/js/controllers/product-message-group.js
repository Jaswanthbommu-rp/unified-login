//  Property Groups Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProductMessageGroupsGridCtrl($scope, $filter, dataSvc, gridModel, gridTransformSvc, gridPaginationModel, security, persona, syncMgr, productDataModel, userDetailsModel) {
        var vm = this,
            pmgGrid = gridModel(),
            pmgGridTransform = gridTransformSvc(),
            pmgGridPagination = gridPaginationModel();

        vm.init = function () {
            vm.grid = pmgGrid;
            vm.propertyGroupsError = $filter("productPanelText")("panelError.generic");
            vm.config = syncMgr.getProductGridConfig($scope.$parent.productId, "PropertyGroup");
            pmgGridTransform.watch(pmgGrid);
            pmgGrid.setConfig(vm.config);
            pmgGridPagination.setGrid(pmgGrid);
            $scope.pmgGridPagination = pmgGridPagination;

            pmgGridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.gridSelectionWatch = pmgGrid.subscribe("selectChange", vm.selectionChange);
            vm.gridSelectAllWatch = pmgGrid.subscribe("selectAll", vm.selectAllPropertyGroup);
            vm.filterData = pmgGrid.subscribe("filterBy", vm.filter.bind(vm));
        };

        vm.isActive = function () {
            return productDataModel.isPropertyGridActive();
        };

        vm.hasViewOnlyAccess = function () {
            return security.isAllowed("viewUser") || syncMgr.isUserHasManageProductAccess($scope.$parent.productId);
        };

        vm.updateGrid = function () {
            vm.grid.updateSelected();
        };

        vm.filter = function(filterBy){
            vm.filteredRecords = $filter("filter")(vm.dataReq.records, filterBy);
        };

        vm.selectionAll = function (val) {
            syncMgr.allPropertiesSync($scope.$parent.productId, val);
            vm.updateGrid();
        };

        vm.selectionChange = function (record) {
            if (record) {
                syncMgr.groupToPropertySync($scope.$parent.productId, record);
            }
        };

        vm.loadData = function () {
            var productId = $scope.$parent.productId;

            pmgGrid.busy(true);
            if (persona.isReady() && vm.isActive()) {
                var propertyData = syncMgr.getMessageGroupMap(productId);

                if (propertyData === undefined) {

                    var params = {
                        userPersonaId: userDetailsModel.getPersonaId(),
                        editorPersonaId: persona.getId()
                    };

                    vm.dataPropReq = dataSvc.get(params, vm.setPropertyGroupData);
                }
                else {
                    vm.loadGridData(productId);
                }
            }
        };

        vm.loadGridData = function (productId) {
            pmgGrid.busy(false);

            var propData = syncMgr.getMessageGroupMap(productId);

            if (propData && propData.length > 0) {
                if (vm.hasViewOnlyAccess()) {
                    propData.forEach(function (item) {
                        angular.extend(item, {
                            disabled: false,
                            radname: "messageGroup"
                        });
                        item.disabled = true;
                    });
                }

                propData.forEach(function (item) {
                    angular.extend(item, {
                        radname: "messageGroup",
                        productId: productId,
                        originalProperty: item.isAssigned
                    });

                });

                pmgGridPagination.setData(propData).goToPage({
                    number: 0
                });
            }

            return vm;
        };

        vm.setPropertyGroupData = function (resp) {
            pmgGrid.busy(false);
            if (resp.records && resp.records.length) {
                var pdata = syncMgr.setMessageGroupList(resp.records, $scope.$parent.productId);
                vm.loadGridData($scope.$parent.productId);
            }

            if (resp.isError) {
                vm.isPropertyGroupsError = true;
            }
        };



        vm.destroy = function () {
            vm.destWatch();
            vm.activeWatch();
            vm.gridSelectionWatch();
            vm.gridSelectAllWatch();
            pmgGrid.destroy();

            if (vm.dataPropReq) {
                vm.dataPropReq.$cancelRequest();
            }

            pmgGridTransform.destroy();
            pmgGridPagination.destroy();
            pmgGrid = undefined;
            pmgGridTransform = undefined;
            pmgGridPagination = undefined;
            //vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProductMessageGroupsGridCtrl", [
            "$scope",
            "$filter",
            "RPMessagingGroupsSvc",
            "rpGridModel",
            "rpGridTransform",
            "rpGridPaginationModel",
            "routeSecurity",
            "personaDetails",
            "productDataSyncManager",
            "productPanelDataModel",
            "userDetailsModel",
            ProductMessageGroupsGridCtrl
        ]);
})(angular);

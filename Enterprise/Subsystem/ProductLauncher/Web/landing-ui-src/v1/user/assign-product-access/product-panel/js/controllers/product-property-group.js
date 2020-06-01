//  Property Groups Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProductPropertyGroupsGridCtrl($scope, $filter, dataSvc, gridModel, gridTransformSvc, gridPaginationModel, security, persona, syncMgr, productDataModel, userDetailsModel) {
        var vm = this,
            userLoginName = "",
            pgGrid = gridModel(),
            pgGridTransform = gridTransformSvc(),
            pgGridPagination = gridPaginationModel();

        vm.init = function () {
            vm.grid = pgGrid;
            vm.propertyGroupsError = $filter("productPanelText")("panelError.generic");
            vm.config = syncMgr.getProductGridConfig($scope.$parent.productId, "PropertyGroup");
            pgGridTransform.watch(pgGrid);
            pgGrid.setConfig(vm.config);
            pgGridPagination.setGrid(pgGrid);
            $scope.pgGridPagination = pgGridPagination;

            pgGridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.gridSelectionWatch = pgGrid.subscribe("selectChange", vm.selectionChange);
            vm.gridSelectAllWatch = pgGrid.subscribe("selectAll", vm.selectAllPropertyGroup);
            vm.filterData = pgGrid.subscribe("filterBy", vm.filter.bind(vm));
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

        vm.selectAllPropertyGroup = function (val) {
            logc("group recordselectall", val);
            var productId = $scope.$parent.productId;
            if(productId != 18){
                syncMgr.allPropertiesSync($scope.$parent.productId, val);
            }
            vm.updateGrid();
        };

        vm.selectionChange = function (record) {
            if (record) {
                syncMgr.groupToPropertySync($scope.$parent.productId, record);
            }
        };

        vm.loadData = function () {
            var productId = $scope.$parent.productId;

            pgGrid.busy(true);
            if (persona.isReady() && vm.isActive()) {
                var propertyData = syncMgr.getProductPropertiesData(productId);

                if (propertyData === undefined) {

                    var params = {
                        userPersonaId: userDetailsModel.getPersonaId(),
                        editorPersonaId: persona.getId(),
                        productId: productId,
                        userLoginName: userDetailsModel.getLoginName() === undefined ? userLoginName : userDetailsModel.getLoginName()
                    };

                    vm.dataPropReq = dataSvc.get(params, vm.setPropertyGroupData);
                }
                else {
                    vm.loadGridData(productId);
                }
            }
        };

        vm.loadGridData = function (productId) {
            pgGrid.busy(false);

            var propData = syncMgr.getProductPropertyGroupData(productId);

            if (propData && propData.length > 0) {
                if (vm.hasViewOnlyAccess()) {
                    propData.forEach(function (item) {
                        angular.extend(item, {
                            disabled: false,
                            radname: "propertyGroup"
                        });
                        item.disabled = true;
                    });
                }

                propData.forEach(function (item) {
                    angular.extend(item, {
                        radname: "propertyGroup",
                        productId: productId,
                        originalProperty: item.isAssigned
                    });

                });

                pgGridPagination.setData(propData).goToPage({
                    number: 0
                });
            }

            return vm;
        };

        vm.setPropertyGroupData = function (resp) {
            pgGrid.busy(false);
            if (resp.records && resp.records.length) {
                var pdata = syncMgr.setPropertyGroupList(resp.records, $scope.$parent.productId);
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
            pgGrid.destroy();

            if (vm.dataPropReq) {
                vm.dataPropReq.$cancelRequest();
            }

            pgGridTransform.destroy();
            pgGridPagination.destroy();
            pgGrid = undefined;
            pgGridTransform = undefined;
            pgGridPagination = undefined;
            //vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProductPropertyGroupsGridCtrl", [
            "$scope",
            "$filter",
            "productPropertyGroupSvc",
            "rpGridModel",
            "rpGridTransform",
            "rpGridPaginationModel",
            "routeSecurity",
            "personaDetails",
            "productDataSyncManager",
            "productPanelDataModel",
            "userDetailsModel",
            ProductPropertyGroupsGridCtrl
        ]);
})(angular);

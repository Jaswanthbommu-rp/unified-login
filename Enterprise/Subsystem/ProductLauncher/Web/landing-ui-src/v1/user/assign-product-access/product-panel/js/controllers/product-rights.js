//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProductRightsGridCtrl($scope, $filter, gridModel, gridTransformSvc, gridPaginationModel, persona, pubsub, productDataModel, userDetailsModel, security, syncMgr, rightSvc, dependencySvc, tabsModel, menuConfig) {
        var vm = this,
            rightsGrid = gridModel(),
            rightsGridTransform = gridTransformSvc(),
            rightsGridPagination = gridPaginationModel(),
            genericDataErrorReason = "",
            userLoginName = "",
            selectconfigs = [],
            allRightsData = [];

        vm.init = function () {
            vm = this;
            vm.grid = rightsGrid;
            vm.allRightsData = allRightsData;

            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            logc("syncMgr.getProductGridConfig", syncMgr.getProductGridConfig($scope.$parent.productId, "Rights"));
            vm.config = syncMgr.getProductGridConfig($scope.$parent.productId, "Rights");
            rightsGridTransform.watch(rightsGrid);
            rightsGrid.setConfig(vm.config);
            rightsGridPagination.setGrid(rightsGrid);
            $scope.rightsGridPagination = rightsGridPagination;

            rightsGridPagination.setConfig({
                recordsPerPage: 25
            });
            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isReady, vm.loadData);
            vm.productSelectTypeWatch = $scope.$watch(vm.isSelectTypeConfigLoaded, vm.setSelectTypeConfig);
            vm.filterData = rightsGrid.subscribe("filterBy", vm.filter.bind(vm));
            // pubsub.subscribe("ppanel.role-radio", vm.updateRoleRecords);
            vm.gridAllWatch = rightsGrid.subscribe("selectAll", vm.selectionAll);
            vm.gridSelectionWatch = rightsGrid.subscribe("selectChange", vm.updateMultiSelectRoleRecords);
        };

        vm.isActive = function () {
            return productDataModel.isRoleGridActive();
        };
        vm.filter = function (filterBy) {
            vm.filteredRecords = $filter("filter")(vm.allRightsData, filterBy);
            rightsGridPagination.setData(vm.filteredRecords).goToPage({
                number: 0
            });
        };

        vm.isReady = function () {
            return productDataModel.isRoleGridActive(); //productDataModel.isActive();
        };

        vm.isSelectTypeConfigLoaded = function () {
            return syncMgr.isSelectTypeConfigLoaded() && vm.isReady();
        };

        vm.hasViewOnlyAccess = function () {
            return security.isAllowed("viewUser") || syncMgr.isUserHasManageProductAccess($scope.$parent.productId);
        };

        vm.loadData = function () {
            var productId = $scope.$parent.productId;
            rightsGrid.busy(true);
            if (persona.isReady() && vm.isActive()) {
                var rightData = syncMgr.getProductRightsData(productId);
                logc("additionalrightdata", rightData, productId);
                if (rightData === undefined) {

                    var params = {
                        userPersonaId: userDetailsModel.getPersonaId(),
                        editorPersonaId: persona.getId(),
                        partyId: persona.data.organization.partyId,
                        productId: productId,
                        userLoginName: userDetailsModel.getLoginName() === undefined ? userLoginName : userDetailsModel.getLoginName()
                    };

                    vm.dataRightReq = rightSvc.get(params, vm.setRightsData);
                }
                else {
                    vm.loadGridData(productId);
                }
            }
        };
        vm.loadGridData = function (productId) {
            rightsGrid.busy(false);
            var rightData = syncMgr.getProductRightsData(productId);
            if (rightData && rightData.length > 0) {
                vm.allRightsData = rightData;
                if (vm.hasViewOnlyAccess()) {
                    rightData.forEach(function (item) {
                        angular.extend(item, {
                            disabled: false,
                            radname: "right"
                        });
                        item.disabled = true;
                    });
                }

                rightData.forEach(function (item) {
                    angular.extend(item, {
                        radname: "right",
                        productId: productId,
                        originalProperty: item.isAssigned
                    });

                });
                rightsGridPagination.setData(rightData).goToPage({
                    number: 0
                });

            }

            return vm;
        };
        vm.setRightsData = function (resp) {
            rightsGrid.busy(false);
            if (resp.records && resp.records.length > 0) {
                var rdata = syncMgr.setRightList(resp.records, $scope.$parent.productId);
                // if (resp.additional && resp.additional.presets) {
                //     syncMgr.setPresetRoleList(resp.additional.presets, $scope.$parent.productId);
                // }
                vm.loadGridData($scope.$parent.productId);
            }

            if (resp.isError) {
                vm.isDataError = true;
                if (resp.errorReason !== "") {
                    vm.dataErrorReason = resp.errorReason;
                }
                else {
                    vm.dataErrorReason = genericDataErrorReason;
                }
            }

        };
        vm.selectionAll = function (bool) {
            if (vm.filteredRecords !== undefined) {
                syncMgr.updateAllFilterRights($scope.$parent.productId, vm.filteredRecords, bool);
            }
            else {
                syncMgr.updateAllFilterRights($scope.$parent.productId, vm.allRightsData, bool);
            }
        };
        vm.updateMultiSelectRoleRecords = function (record) {
            if (record) {
                syncMgr.multiSelectRightSync(record.productId, record);
            }
        };

        vm.destroy = function () {
            logc("destroy called");
            vm.destWatch();
            vm.personaWatch();
            vm.activeWatch();
            vm.gridAllWatch();
            vm.gridSelectionWatch();
            // vm.productSelectTypeWatch();
            if (vm.dataRightReq) {
                vm.dataRightReq.$cancelRequest();
            }

            if (vm.dataCntrlDependencyReq) {
                vm.dataCntrlDependencyReq.$cancelRequest();
            }
            rightsGrid.destroy();
            rightsGridTransform.destroy();
            rightsGridPagination.destroy();
            rightsGrid = undefined;
            rightsGridTransform = undefined;
            rightsGridPagination = undefined;
            vm.roleRights = [];
            // vm.productRoleSelectedWatch();
            //vm = undefined;
            //$scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProductRightsGridCtrl", [
            "$scope",
            "$filter",
            "rpGridModel",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "pubsub",
            "productPanelDataModel",
            "userDetailsModel",
            "routeSecurity",
            "productDataSyncManager",
            "productRightsSvc",
            "productControlDependencySvc",
            "productPanelTabsModel",
            "rpFormSelectMenuConfig",
            ProductRightsGridCtrl
        ]);
})(angular);

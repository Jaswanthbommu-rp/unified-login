//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProductBenchMarkRolesGridCtrl($scope, $filter, gridModel, gridTransformSvc, gridPaginationModel, persona, pubsub, productDataModel, userDetailsModel, security, syncMgr, roleSvc) {
        var vm = this,
            bmrolesGrid = gridModel(),
            bmrolesGridTransform = gridTransformSvc(),
            bmrolesGridPagination = gridPaginationModel(),
            genericDataErrorReason = "",
            assignedRoleId = 0,
            userLoginName = "";

        vm.init = function () {
            vm = this;
            vm.bmrolesGrid = bmrolesGrid;
            vm.assignedRoleId = 0;


            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            bmrolesGridTransform.watch(bmrolesGrid);
            bmrolesGrid.setConfig(syncMgr.getProductGridConfig(34, "BenchmarkingRole"));

            bmrolesGridPagination.setGrid(bmrolesGrid);
            $scope.bmrolesGridPagination = bmrolesGridPagination;
            bmrolesGridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isReady, vm.loadData);

            vm.gridAllWatch = bmrolesGrid.subscribe("selectAll", vm.selectionAll);
            vm.gridSelectionWatch = bmrolesGrid.subscribe("selectChange", vm.updateMultiSelectRoleRecords);
        };

        vm.isActive = function () {
            return productDataModel.isRoleGridActive();
        };

        vm.isReady = function () {
            return productDataModel.isRoleGridActive(); //productDataModel.isActive();
        };


        vm.hasViewOnlyAccess = function () {
            return security.isAllowed("viewUser") || syncMgr.isUserHasManageProductAccess($scope.$parent.productId);
        };

        vm.loadData = function () {
            var productId = 34;
            bmrolesGrid.busy(true);
            if (persona.isReady() && vm.isActive()) {
                var roleData = syncMgr.getProductBenchMarkRolesData(productId);
                 logc("bmroleData",roleData,productId);
                if (roleData === undefined) {

                    var params = {
                        userPersonaId: userDetailsModel.getPersonaId(),
                        editorPersonaId: persona.getId(),
                        partyId: persona.data.organization.partyId,
                        productId: productId,
                        userLoginName: userDetailsModel.getLoginName() === undefined ? userLoginName : userDetailsModel.getLoginName()
                    };

                    vm.dataBMRoleReq = roleSvc.get(params, vm.setRolesData);
                }
                else {
                    //syncMgr.setPropertyGridActive(true);
                    vm.loadGridData(productId);
                }
            }
        };


        vm.loadGridData = function (productId) {
            //var productId = $scope.$parent.productId;
            bmrolesGrid.busy(false);
            var roleData = syncMgr.getProductBenchMarkRolesData(productId);
logc("syncMgr",syncMgr,roleData);
            if (roleData && roleData.length > 0) {
                if (vm.hasViewOnlyAccess()) {
                    roleData.forEach(function (item) {
                        angular.extend(item, {
                            disabled: false,
                            radname: "role"
                        });
                        item.disabled = true;
                    });
                }

                roleData.forEach(function (item) {
                    angular.extend(item, {
                        radname: "role",
                        productId: productId
                    });
                });


                bmrolesGridPagination.setData(roleData).goToPage({
                    number: 0
                });

            }

            return vm;
        };

         vm.setRolesData = function (resp) {
            bmrolesGrid.busy(false);
            if (resp.records && resp.records.length > 0) {
                var rdata = syncMgr.setBenchMarkRoleList(resp.records, 34);
                vm.loadGridData(34);
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
            var data = syncMgr.allBenchMarkRolesSync(34, bool);
        };

        vm.updateRoleRecords = function (record) {
            bmrolesGrid.busy(true);
            var rolesData = syncMgr.selectedBenchMarkRoleSync(record.productId, record);
            bmrolesGrid.busy(false);
        };

        vm.updateMultiSelectRoleRecords = function (record) {
            if (record) {
                syncMgr.multiSelectBenchMarkRoleSync(record.productId, record);
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
            if (vm.dataBMRoleReq) {
                vm.dataBMRoleReq.$cancelRequest();
            }
            bmrolesGrid.destroy();
            bmrolesGridTransform.destroy();
            bmrolesGridPagination.destroy();
            bmrolesGrid = undefined;
            bmrolesGridTransform = undefined;
            bmrolesGridPagination = undefined;
            // vm.productRoleSelectedWatch();
            //vm = undefined;
            //$scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProductBenchMarkRolesGridCtrl", [
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
            "productRolesSvc",
            ProductBenchMarkRolesGridCtrl
        ]);
})(angular);

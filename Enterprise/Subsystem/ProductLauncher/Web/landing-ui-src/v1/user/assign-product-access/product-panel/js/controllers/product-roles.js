//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProductRolesGridCtrl($scope, $filter, gridModel, gridTransformSvc, gridPaginationModel, persona, pubsub, productDataModel, userDetailsModel, security, configModel, syncMgr, roleSvc) {
        var vm = this,
            rolesGrid = gridModel(),
            rolesGridTransform = gridTransformSvc(),
            roleGridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.rolesGrid = rolesGrid;

            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            rolesGridTransform.watch(rolesGrid);

            vm.config = configModel.getGridConfig().length > 1 ? configModel.getGridConfig()[1] : configModel.getGridConfig()[0];
            rolesGrid.setConfig(vm.config);

            roleGridPagination.setGrid(rolesGrid);
            $scope.roleGridPagination = roleGridPagination;
            roleGridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isReady, vm.loadData);

            pubsub.subscribe("ppanel.role-radio", vm.updateRoleRecords);
            vm.gridAllWatch = rolesGrid.subscribe("selectAll", vm.selectionAll);
            vm.gridSelectionWatch = rolesGrid.subscribe("selectChange", vm.updateMultiSelectRoleRecords);
        };

        vm.isActive = function () {
            return true; //productDataModel.isActive();
        };

        vm.isReady = function () {
            return productDataModel.isRoleGridActive(); //productDataModel.isActive();
        };

        vm.hasViewOnlyAccess = function () {
            return security.isAllowed("viewUser") || syncMgr.isUserHasManageProductAccess($scope.$parent.productId);
        };

        vm.updateRoleRecords = function (record) {
            var rolesData = syncMgr.selectedRoleSync(record.productId, record);
        };

        vm.loadData = function () {
            var productId = $scope.$parent.productId;
            rolesGrid.busy(true);
            if (persona.isReady() && vm.isActive()) {
                var roleData = syncMgr.getProductRolesData(productId);
                // logc("propertyData",propertyData,productId);
                if (roleData === undefined) {

                    var params = {
                        userPersonaId: userDetailsModel.getPersonaId(),
                        editorPersonaId: persona.getId(),
                        partyId: persona.data.organization.partyId,
                        productId: productId
                    };

                    vm.dataRoleReq = roleSvc.get(params, vm.setRolesData);
                }
                else {
                    //syncMgr.setPropertyGridActive(true);
                    vm.loadGridData(productId);
                }
            }
            rolesGrid.busy(false);
        };

        vm.setRolesData = function (resp) {
            // var productId = $scope.$parent.productId;
            if (resp.records && resp.records.length > 0) {
                // logc("setPropertyData",resp.records, vm.productId);
                var rdata = syncMgr.setRoleList(resp.records, $scope.$parent.productId);
                //syncMgr.setPropertyGridActive(true);
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


        vm.loadGridData = function (productId) {
            //var productId = $scope.$parent.productId;
            var roleData = syncMgr.getProductRolesData(productId);
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

                roleGridPagination.setData(roleData).goToPage({
                    number: 0
                });

                //productDataModel.setRoles(resp.records);
            }

            return vm;
        };

        vm.selectionAll = function (bool) {
            var data = syncMgr.allRolesSync($scope.$parent.productId, bool);
        };

        vm.updateMultiSelectRoleRecords = function (record) {
            if (record) {
                var propertiesData = syncMgr.multiSelectedRoleSync(record.productId, record);
            }
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.personaWatch();
            vm.activeWatch();
            vm.gridAllWatch();
            vm.gridSelectionWatch();
            if (vm.dataRoleReq) {
                vm.dataRoleReq.$cancelRequest();
            }
            rolesGrid.destroy();
            rolesGridTransform.destroy();
            roleGridPagination.destroy();
            rolesGrid = undefined;
            rolesGridTransform = undefined;
            roleGridPagination = undefined;
            // vm.productRoleSelectedWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProductRolesGridCtrl", [
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
            "ConfigModel",
            "productDataSyncManager",
            "productRolesSvc",
            ProductRolesGridCtrl
        ]);
})(angular);

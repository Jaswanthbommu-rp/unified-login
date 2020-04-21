//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProductRolesGridCtrl($scope, $filter, gridModel, gridTransformSvc, gridPaginationModel, persona, pubsub, productDataModel, userDetailsModel, security, syncMgr, roleSvc, dependencySvc, tabsModel, menuConfig) {
        var vm = this,
            rolesGrid = gridModel(),
            rolesGridTransform = gridTransformSvc(),
            roleGridPagination = gridPaginationModel(),
            genericDataErrorReason = "",
            assignedRoleId = 0,
            roleRights = [];

        vm.init = function () {
            vm = this;
            vm.rolesGrid = rolesGrid;
            vm.assignedRoleId = 0;
            vm.roleRights = [];
            vm.presetRoles = [];
            vm.roleSelected = "";

            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            rolesGridTransform.watch(rolesGrid);

            rolesGrid.setConfig(syncMgr.getProductGridConfig($scope.$parent.productId, "Roles"));

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
        };

        vm.loadProductControlDependencyData = function (controlId) {
            var params = {
                controlId: controlId
            };
            vm.dataCntrlDependencyReq = dependencySvc.get(params, vm.setControlDependencyData);
        };

        vm.loadGridData = function (productId) {
            //var productId = $scope.$parent.productId;
            rolesGrid.busy(false);
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

                    if (item.isAssigned && item.userRights !== undefined) {
                        vm.assignedRoleId = item.roleId;
                        vm.roleRights = item.userRights;
                        // if (item.userRights !== undefined) {
                        //     vm.roleRights = item.userRights;
                        // }
                        //logc("record.userRights11111", vm.roleRights, vm);
                    }
                });

                var dependencyControlId = syncMgr.getProductDependencyControlId(productId, 'role');

                if (dependencyControlId > 0) {
                    vm.loadProductControlDependencyData(dependencyControlId);
                }

                roleGridPagination.setData(roleData).goToPage({
                    number: 0
                });

            }

            return vm;
        };

        vm.selectionAll = function (bool) {
            var data = syncMgr.allRolesSync($scope.$parent.productId, bool);
        };

        vm.setRolesData = function (resp) {
            rolesGrid.busy(false);
            if (resp.records && resp.records.length > 0) {
                var rdata = syncMgr.setRoleList(resp.records, $scope.$parent.productId);
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

         vm.setSelectTypeConfig = function () {
            var productId = $scope.$parent.productId;
            vm.selectconfigs = syncMgr.getProductSelectTypeConfig(productId, "Roles");

            if (vm.selectconfigs !== undefined && vm.selectconfigs.length > 0) {
                vm.selectconfigs.forEach(function (item) {
                    item.configData = menuConfig({
                        onChange: vm.roleSelectedChange,
                        disabled: vm.hasViewOnlyAccess()
                        //productId == 9 ? vm.updateNewPropertyByDefault :
                    });
                });
            }
        };

        vm.setControlDependencyData = function (resp) {
            logc("setControlDependencyData", resp.data);
            //var roleRights = productDataModel.getRoleRights();
            //logc("vm.roleRights", vm.roleRights);
            if (resp.data && resp.data.length > 0 && vm.roleRights.length > 0) {
                var matchFound = false;
                vm.roleRights.forEach(function (right) {
                    var record = resp.data.filter(function (data) {
                        return right.rightNickName.toLowerCase() === data.masterControlValue.toLowerCase();
                    })[0];

                    if (record !== undefined && record) {
                        matchFound = true;
                    }
                });

                var tabs = syncMgr.getProductInitialTabs($scope.$parent.productId);
                if (matchFound) {
                    tabs = syncMgr.getProductAllTabs($scope.$parent.productId);
                }

                var activeTab = syncMgr.getProductActiveTab($scope.$parent.productId);
                tabsModel.setTabs(tabs);
                tabsModel.setTabMenuData(tabs);
                tabsModel.activateTab(activeTab).initActiveTab();
            }
        };

        vm.updateRoleRecords = function (record) {
            var rolesData = syncMgr.selectedRoleSync(record.productId, record);
            if (record.isAssigned && record.userRights !== undefined) {
                vm.roleRights = [];
                if (record.userRights !== undefined) {
                    vm.roleRights = record.userRights;
                }
                var dependencyControlId = syncMgr.getProductDependencyControlId(record.productId, record.radname);

                if (dependencyControlId > 0) {
                    vm.loadProductControlDependencyData(dependencyControlId);
                }
            }
        };

        vm.updateMultiSelectRoleRecords = function (record) {
            if (record) {
                var propertiesData = syncMgr.multiSelectedRoleSync(record.productId, record);
            }
        };

        vm.destroy = function () {
            logc("destroy called");
            vm.destWatch();
            vm.personaWatch();
            vm.activeWatch();
            vm.gridAllWatch();
            vm.gridSelectionWatch();
            if (vm.dataRoleReq) {
                vm.dataRoleReq.$cancelRequest();
            }

            if (vm.dataCntrlDependencyReq) {
                vm.dataCntrlDependencyReq.$cancelRequest();
            }
            rolesGrid.destroy();
            rolesGridTransform.destroy();
            roleGridPagination.destroy();
            rolesGrid = undefined;
            rolesGridTransform = undefined;
            roleGridPagination = undefined;
            vm.roleRights = [];
            // vm.productRoleSelectedWatch();
            //vm = undefined;
            //$scope = undefined;
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
            "productDataSyncManager",
            "productRolesSvc",
            "productControlDependencySvc",
            "productPanelTabsModel",
            "rpFormSelectMenuConfig",
            ProductRolesGridCtrl
        ]);
})(angular);

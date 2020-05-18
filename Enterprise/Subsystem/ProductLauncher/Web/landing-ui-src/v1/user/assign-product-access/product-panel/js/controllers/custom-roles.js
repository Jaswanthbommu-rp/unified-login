//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProductCustomRolesGridCtrl($scope, $filter, gridModel, gridTransformSvc, gridPaginationModel, persona, pubsub, productDataModel, userDetailsModel, security, syncMgr, roleSvc, dependencySvc, tabsModel, menuConfig) {
        var vm = this,
            customRolesGrid = gridModel(),
            customRolesGridTransform = gridTransformSvc(),
            customRoleGridPagination = gridPaginationModel(),
            genericDataErrorReason = "",
            assignedRoleId = 0,
            rolelblName = [],
            selectconfigs = [];

        vm.init = function () {
            vm = this;
            vm.customRolesGrid = customRolesGrid;
            vm.assignedRoleId = 0;
            vm.rolelblName = "";
            vm.presetRoles = [];
            vm.roleSelected = "";
            vm.selectconfigs = [];
            vm.customRolesData= [
                                {id: "1", name: "Property Manager", isAssigned: false},
                                {id: "2", name: "Group Manager", isAssigned: false},
                                {id: "3", name: "Portfolio Manager", isAssigned: false}
                            ];
            vm.isPortfolioBtn = false;

            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            customRolesGridTransform.watch(customRolesGrid);
            customRolesGrid.setConfig(syncMgr.getProductGridConfig($scope.$parent.productId, "Roles"));

            customRoleGridPagination.setGrid(customRolesGrid);
            $scope.customRoleGridPagination = customRoleGridPagination;
            customRoleGridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isReady, vm.loadData);
            vm.productSelectTypeWatch = $scope.$watch(vm.isSelectTypeConfigLoaded, vm.setSelectTypeConfig);

            pubsub.subscribe("ppanel.role-radio", vm.updateRoleRecords);
            vm.gridAllWatch = customRolesGrid.subscribe("selectAll", vm.selectionAll);
            vm.gridSelectionWatch = customRolesGrid.subscribe("selectChange", vm.updateMultiSelectRoleRecords);
        };

        vm.isActive = function () {
            return true; //productDataModel.isActive();
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
            customRolesGrid.busy(true);
            if (persona.isReady() && vm.isActive()) {
                var roleData = syncMgr.getProductCustomRolesData(productId);
                if (roleData === undefined) {

                    var params = {
                        userPersonaId: userDetailsModel.getPersonaId(),
                        editorPersonaId: persona.getId(),
                        partyId: persona.data.organization.partyId,
                        productId: productId
                    };

                    // vm.dataRoleReq = roleSvc.get(params, vm.setRolesData);
                    vm.setRolesData(vm.customRolesData);
                }
                else {
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
            customRolesGrid.busy(false);
            var roleData = syncMgr.getProductCustomRolesData(productId);
             var presetroleData = syncMgr.getProductPresetRolesData(productId);
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

                    if (item.isAssigned && item.name !== undefined) {
                        vm.assignedRoleId = item.id;
                        vm.rolelblName = item.name.replace(/[\s]/g, '');
                    }
                });

                var dependencyControlId = syncMgr.getProductDependencyControlId(productId, 'role');

                if (dependencyControlId > 0) {
                    vm.loadProductControlDependencyData(dependencyControlId);
                }
                else if ($scope.$parent.productId === 3 || $scope.$parent.productId === 17) {
                    var tabs = syncMgr.getProductInitialTabs($scope.$parent.productId);
                    vm.setProductTabs(tabs);
                }

                if (presetroleData !== undefined) {
                    vm.presetRoles = [];
                    var defaultData = {
                        "id": "",
                        "name": "Select a Preset Role",
                        "roleIds": []
                    };

                    vm.presetRoles.push(defaultData);
                    presetroleData.forEach(function (option) {
                        vm.presetRoles.push(option);
                    });
                    logc("vm.presetRoles data", vm.presetRoles);
                    //vm.roleSelect.setOptions(vm.presetRoles);
                    // logc("vm.selectconfigs data", vm.selectconfigs);
                    vm.selectconfigs.forEach(function (item) {
                        item.configData.setOptions(vm.presetRoles);
                        logc("item.configData", item.configData);
                    });
                    // vm.selectconfigs[0].configData.setOptions(vm.presetRoles);
                    logc("presetroleData", vm.selectconfigs);
                }

                customRoleGridPagination.setData(roleData).goToPage({
                    number: 0
                });

            }

            return vm;
        };

        vm.roleSelectedChange = function (val) {
            logc("presetroleselected", val);
            //var roleData = syncMgr.getProductRolesData(productId);
            var roleSelected = vm.presetRoles.find(function (item) {
                return item.id === val;
            });
            //presetModel.setData(roleSelected);

            // Clear any selections
            vm.customRolesGrid.selectAll(false);
            //vm.customRolesGrid.updateSelected();

            // Select defaults based on role
            // vm.rights.forEach(function (item) {
            //     item.isAssigned = presetModel.containsId(item.id);
            // });
            //var presetroleData = syncMgr.getProductPresetRolesData($scope.$parent.productId);
            //logc("presetroleData", presetroleData, roleSelected);
            syncMgr.setSelectedPresetRoleSync($scope.$parent.productId, roleSelected.roleIds);
            vm.customRolesGrid.updateSelected();
        };

        vm.selectionAll = function (bool) {
            var data = syncMgr.allRolesSync($scope.$parent.productId, bool);
        };

        vm.setRolesData = function (resp) {
            customRolesGrid.busy(false);
            if (resp && resp.length > 0) {
                var rdata = syncMgr.setCustomRoleList(resp, $scope.$parent.productId);
                if (resp.additional && resp.additional.presets) {
                    syncMgr.setPresetRoleList(resp.additional.presets, $scope.$parent.productId);
                }
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
            var configs = syncMgr.getProductSelectTypeConfig(productId, "Roles");
            //logc("vm.selectconfigs from model", vm.selectconfigs);
            if (configs !== undefined && configs.length > 0) {
                configs.forEach(function (item) {
                    // logc("select config item", item);
                    // vm.roleSelect = menuConfig({
                    //     nameKey: "name",
                    //     valueKey: "id",
                    //     fieldName: "roleSelect",
                    //     onChange: vm.roleSelectedChange,
                    //     disabled: vm.hasViewOnlyAccess()
                    // });
                    item.configData = menuConfig({
                        nameKey: "name",
                        valueKey: "id",
                        fieldName: "roleSelect",
                        onChange: vm.roleSelectedChange,
                        disabled: vm.hasViewOnlyAccess()
                    });
                    vm.selectconfigs.push(item);
                });
                //vm.roleSelect = vm.selectconfigs[0].configData;
                // logc("select config vm.roleSelect", vm.roleSelect);
            }
        };

        vm.setControlDependencyData = function (resp) {
            var tabs = syncMgr.getProductInitialTabs($scope.$parent.productId);
            var record;
           // var allTabs=angular.copy(tabs);
            var matchTab;
            if (resp.data && resp.data.length > 0 ) {
                var matchFound = false;
                     record = resp.data.filter(function (data) {
                        return vm.rolelblName.toLowerCase() === data.masterControlValue.toLowerCase();
                    });
                    if (record.length !== 0) {
                        matchFound = true;
                    }
                
                //Exclude properties tab fro employee and external user company
                var compId = persona.getBooksMasterId();
                if (compId === -1 || compId === -2) {
                    matchFound = false;
                }
                if (matchFound) {
                    syncMgr.getProductAllTabs($scope.$parent.productId).forEach(function (item) {
                        record.filter(function (data) {
                            if(item.id.toLowerCase() === data.displayName.toLowerCase()){
                                tabs.push(item);
                            }
                        });
                    });
                }
                syncMgr.setProductDependencyDataMap($scope.$parent.productId, matchFound);
                vm.setProductTabs(tabs);
            }
            else {
                vm.setProductTabs(tabs);
            }
        };

         vm.setProductTabs = function (tabs) {
            var activeTab = syncMgr.getProductActiveTab($scope.$parent.productId);
            tabsModel.setTabs(tabs);
            tabsModel.setTabMenuData(tabs);
            tabsModel.activateTab(activeTab).initActiveTab();
        };

        vm.updateRoleRecords = function (record) {
            var rolesData = syncMgr.selectedRoleSync(record.productId, record);
            if (record.isAssigned && record.name !== undefined && record.name !== "Portfolio Manager") {
                vm.isPortfolioBtn= false;
                vm.rolelblName = [];
                if (record.name !== undefined) {
                    vm.rolelblName = record.name.replace(/[\s]/g, '');
                }
                var dependencyControlId = syncMgr.getProductDependencyControlId(record.productId, record.radname);

                if (dependencyControlId > 0) {
                    vm.loadProductControlDependencyData(dependencyControlId);
                }
            }
            else{
                vm.isPortfolioBtn= true;
            }
        };

        vm.updateMultiSelectRoleRecords = function (record) {
            if (record) {
                syncMgr.multiSelectedRoleSync(record.productId, record);
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
            if (vm.dataRoleReq) {
                vm.dataRoleReq.$cancelRequest();
            }

            if (vm.dataCntrlDependencyReq) {
                vm.dataCntrlDependencyReq.$cancelRequest();
            }
            customRolesGrid.destroy();
            customRolesGridTransform.destroy();
            customRoleGridPagination.destroy();
            customRolesGrid = undefined;
            customRolesGridTransform = undefined;
            customRoleGridPagination = undefined;
            vm.rolelblName = "";
            // vm.productRoleSelectedWatch();
            //vm = undefined;
            //$scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProductCustomRolesGridCtrl", [
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
            ProductCustomRolesGridCtrl
        ]);
})(angular);

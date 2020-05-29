//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProductRolesGridCtrl($scope, $filter, gridModel, gridTransformSvc, gridPaginationModel, persona, pubsub, productDataModel, userDetailsModel, security, syncMgr, roleSvc, dependencySvc, tabsModel, menuConfig, switchConfig) {
        var vm = this,
            rolesGrid = gridModel(),
            rolesGridTransform = gridTransformSvc(),
            roleGridPagination = gridPaginationModel(),
            genericDataErrorReason = "",
            assignedRoleId = 0,
            roleRights,
            userLoginName = "",
            selectconfigs = [],
            isSelectAllPropMsg1;

        vm.init = function () {
            vm = this;
            vm.rolesGrid = rolesGrid;
            vm.assignedRoleId = 0;
            vm.roleRights = [];
            vm.presetRoles = [];
            vm.roleSelected = {};
            vm.rpRoleSelected = "";
            vm.selectconfigs = [];
            vm.isSelectAllPropMsg1 = false;
            vm.allProperties = false;
            vm.showAllPropertiesSwtich = false;


            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            rolesGridTransform.watch(rolesGrid);
            logc("syncMgr.getProductGridConfig", syncMgr.getProductGridConfig($scope.$parent.productId, "Roles"));
            vm.productPropertySwitchWatch = $scope.$watch(vm.isSwitchConfigLoaded, vm.setSwitchConfig);
            rolesGrid.setConfig(syncMgr.getProductGridConfig($scope.$parent.productId, "Roles"));

            roleGridPagination.setGrid(rolesGrid);
            $scope.roleGridPagination = roleGridPagination;
            roleGridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isReady, vm.loadData);
            vm.productSelectTypeWatch = $scope.$watch(vm.isSelectTypeConfigLoaded, vm.setSelectTypeConfig);

            pubsub.subscribe("ppanel.role-radio", vm.updateRoleRecords);
            vm.gridAllWatch = rolesGrid.subscribe("selectAll", vm.selectionAll);
            vm.gridSelectionWatch = rolesGrid.subscribe("selectChange", vm.updateMultiSelectRoleRecords);
        };

        vm.isActive = function () {
            return productDataModel.isRoleGridActive();
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
            rolesGrid.busy(true);
            if (persona.isReady() && vm.isActive()) {
                var roleData = syncMgr.getProductRolesData(productId);
                if (roleData === undefined) {
                    var params = {
                        userPersonaId: userDetailsModel.getPersonaId(),
                        editorPersonaId: persona.getId(),
                        partyId: persona.data.organization.partyId,
                        productId: productId,
                        userLoginName: userDetailsModel.getLoginName() === undefined ? userLoginName : userDetailsModel.getLoginName()
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
            if(productId == 17 || productId == 26){
                vm.rpRoleSelected = roleData.find(function (item) {
                    return item.isAssigned === true;
                });
                logc("vm.rpRoleSelected",vm.rpRoleSelected);
            }
            var presetroleData = syncMgr.getProductPresetRolesData(productId);
            //vm.setSelectTypeConfig();
            //logc("l2lroledata", roleData, presetroleData, rolesGrid);
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

                roleGridPagination.setData(roleData).goToPage({
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
            vm.rolesGrid.selectAll(false);
            //vm.rolesGrid.updateSelected();

            // Select defaults based on role
            // vm.rights.forEach(function (item) {
            //     item.isAssigned = presetModel.containsId(item.id);
            // });
            //var presetroleData = syncMgr.getProductPresetRolesData($scope.$parent.productId);
            //logc("presetroleData", presetroleData, roleSelected);
            syncMgr.setSelectedPresetRoleSync($scope.$parent.productId, roleSelected.roleIds);
            vm.rolesGrid.updateSelected();
        };

        vm.selectionAll = function (bool) {
            var data = syncMgr.allRolesSync($scope.$parent.productId, bool);
        };

        vm.setRolesData = function (resp) {
            rolesGrid.busy(false);
            if (resp.records && resp.records.length > 0) {
                var rdata = syncMgr.setRoleList(resp.records, $scope.$parent.productId);
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
            var productId = $scope.$parent.productId;
            var tabs = syncMgr.getProductInitialTabs(productId);

            if (resp.data && resp.data.length > 0) {
                var matchFound = false;
                if(productId == 3){
                    vm.roleRights.forEach(function (right) {
                        if(right.rightNickName){
                            var record = resp.data.filter(function (data) {
                                return right.rightNickName.toLowerCase() === data.masterControlValue.toLowerCase();
                            })[0];
        
                            if (record !== undefined && record) {
                                matchFound = true;
                            }
                        }
                    });
                }
                else if(productId == 17 || productId == 26){
                    var releventTabs = [];
                    var rpTabs = [];
                    var rpRoleName = "";
                    if(vm.rpRoleSelected){
                        rpRoleName = vm.rpRoleSelected.name.toLowerCase();
                        releventTabs = resp.data.filter(function (data) {
                            return data.masterControlValue.toLowerCase() == rpRoleName;
                        });
                    }                 
                    if (releventTabs.length > 0) {
                        var allTabs = syncMgr.getProductAllTabs($scope.$parent.productId);
                        releventTabs.forEach(function (tb) {
                            var rpTab =  allTabs.find(function (item) {
                                return item.text === tb.displayName;
                            });
                            if(rpTab != undefined){
                                rpTabs.push(rpTab);
                            }
                        });
                        vm.setProductTabs(rpTabs);
                        matchFound = true;
                    
                    }
                    else {
                        vm.setProductTabs(tabs);
                    }
                    if(productId == 17){
                        vm.showAllPropertiesSwtich = (rpRoleName == "enterprise standard") ? true : false;
                        vm.allProperties = ((rpRoleName == "enterprise standard" && syncMgr.isProductAllProperties($scope.$parent.productId)) || rpRoleName == "enterprise admin") ? true : false;
                        syncMgr.updateProductAllProperties($scope.$parent.productId, vm.allProperties);
                    }
                    if (productId == 26) {
                        if(rpTabs.length > 0){
                            vm.setAllProperties(rpTabs);
                        } else{
                            syncMgr.updateProductAllProperties(productId, false);
                        }
                    }
                }              

                //Exclude properties tab fro employee and external user company
                var compId = persona.getBooksMasterId();
                // if (compId === -1 || compId === -2) {
                //     matchFound = false;
                // }
                if(compId === -1 || compId === -2)
                { 
                    matchFound = false;
                    vm.setProductTabs(tabs);
                    
                }else if(productId == 3 ){
                    if(matchFound){
                        tabs = syncMgr.getProductAllTabs($scope.$parent.productId);
                        vm.setProductTabs(tabs);
                    }else{
                        vm.setProductTabs(tabs);
                    }  
                }         
                
                syncMgr.setProductDependencyDataMap($scope.$parent.productId, matchFound);  
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
            vm.isSelectAllPropMsg1 = false;
            if (record.productId == "3" || record.productId == "17" || record.productId == "18" || record.productId == "26") {
                var dependencyControlId = syncMgr.getProductDependencyControlId(record.productId, record.radname);
                if(record.productId == "17"){
                    vm.rpRoleSelected = record;
                    vm.allProperties = false;
                }
                 else if( record.productId == "26"){
                    vm.rpRoleSelected = record;
                }
                else {
                    if (record.isAssigned && record.userRights !== undefined && dependencyControlId > 0) {
                        vm.roleRights = [];
                        if (record.userRights !== undefined) {
                            vm.roleRights = record.userRights;
                        }
                    }
                }
                if(dependencyControlId > 0){
                    vm.loadProductControlDependencyData(dependencyControlId);
                }
            }
        };

        vm.updateMultiSelectRoleRecords = function (record) {
            if (record) {
                syncMgr.multiSelectedRoleSync(record.productId, record);
            }
        };

        vm.isSwitchConfigLoaded = function () {
            return syncMgr.isSwitchConfigLoaded();
        };

        vm.setSwitchConfig = function () {
            var productId = $scope.$parent.productId;
            vm.switchconfigs = syncMgr.getProductSwitchConfig(productId, "Roles");

            if (vm.switchconfigs !== undefined && vm.switchconfigs.length > 0) {
                vm.switchconfigs.forEach(function (item) {
                    item.configData = switchConfig({
                        onChange: vm.setAllProperties,
                        disabled: vm.hasViewOnlyAccess()
                    });
                });
            }
        };

        vm.setAllProperties = function (record) {
            var productId = $scope.$parent.productId;
            if(productId == 17){
                var tabs = syncMgr.getProductInitialTabs($scope.$parent.productId);
                var releventTabs = [];
                if(record){
                    releventTabs = tabs.filter(function (data) {
                        return data.text.toLowerCase() == "roles";
                    });
                }
                if(releventTabs != undefined && releventTabs.length >0){
                    vm.setProductTabs(releventTabs);
                }
                else{
                    vm.setProductTabs(tabs);
                }
                vm.allProperties = record;
                syncMgr.updateProductAllProperties($scope.$parent.productId, record);
            }
            else if(productId == 26 && record.length == 1 && record[0].text == "Roles"){
                vm.isSelectAllPropMsg1 = true;
                syncMgr.updateProductAllProperties($scope.$parent.productId, true);
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
            rolesGrid.destroy();
            rolesGridTransform.destroy();
            roleGridPagination.destroy();
            rolesGrid = undefined;
            rolesGridTransform = undefined;
            roleGridPagination = undefined;
            vm.roleRights = [];
            vm.rpRoleSelected = "";
            vm.allProperties = false;
            vm.showAllPropertiesSwtich = false;
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
            "rpSwitchConfig",
            ProductRolesGridCtrl
        ]);
})(angular);

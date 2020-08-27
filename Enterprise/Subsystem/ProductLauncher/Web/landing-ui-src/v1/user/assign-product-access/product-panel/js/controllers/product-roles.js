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
            roleRights = [],
            userLoginName = "",
            selectconfigs = [],
            isSelectAllPropMsg1,
            isSelectAllPropMsg2;

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
            vm.isSelectAllPropMsg2 = false;
            vm.allProperties = false;
            vm.showAllPropertiesSwitch = false;
            vm.propertySelect =  '';

            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            rolesGridTransform.watch(rolesGrid);
            //logc("syncMgr.getProductGridConfig", syncMgr.getProductGridConfig($scope.$parent.productId, "Roles"));
            vm.productPropertySwitchWatch = $scope.$watch(vm.isSwitchConfigLoaded, vm.setSwitchConfig);
            rolesGrid.setConfig(syncMgr.getProductGridConfig($scope.$parent.productId, "Roles"));

            roleGridPagination.setGrid(rolesGrid);
            $scope.roleGridPagination = roleGridPagination;
            roleGridPagination.setConfig({
                recordsPerPage: 25
            });
            var radioconfig = syncMgr.getProductPageLevelRadioConfig($scope.$parent.productId, "Roles");
            if (radioconfig !== undefined) {
                vm.radioconfig = syncMgr.getProductPageLevelRadioConfig($scope.$parent.productId, "Roles");
            }

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isReady, vm.loadData);
            vm.productSelectTypeWatch = $scope.$watch(vm.isSelectTypeConfigLoaded, vm.setSelectTypeConfig);

            pubsub.subscribe("ppanel.assign-accessType", vm.accessTypeChange);
            pubsub.subscribe("ppanel.role-radio", vm.updateRoleRecords);
            vm.gridAllWatch = rolesGrid.subscribe("selectAll", vm.selectionAll);
            vm.gridSelectionWatch = rolesGrid.subscribe("selectChange", vm.updateMultiSelectRoleRecords);

            vm.updateGridWatch = pubsub.subscribe("rp.updateAllPropertiesSwitchSet",vm.updateAllPropertiesSwitch);
        };

        vm.isActive = function () {
            return productDataModel.isRoleGridActive();
        };

        vm.isReady = function () {
            return productDataModel.isRoleGridActive(); //productDataModel.isActive();
        };

        vm.accessTypeChange = function (accessType) {
            if (accessType === 'specificProperties') {
                vm.propertySelect = 'property';
            }
            else {
                vm.propertySelect = accessType;
            }
            vm.rpRoleSelected = vm.propertySelect;
            vm.resetDataModel(vm.propertySelect);
        };

        vm.updateAllPropertiesSwitch = function(bool){
            if(vm.rpRoleSelected && vm.rpRoleSelected != undefined && vm.rpRoleSelected.name.toLowerCase() == "enterprise admin"){
                vm.showAllPropertiesSwitch = false;
            }
            else{
                vm.showAllPropertiesSwitch = bool;
            }
            vm.allProperties = bool;

            var allTabs = syncMgr.getProductAllTabs($scope.$parent.productId);
            if(bool){
                var tb = allTabs.find(function (item) {
                    return item.text === 'Roles';
                });
                if(tb != undefined){
                    vm.setProductTabs([tb]);
                }
                syncMgr.allPropertiesSync($scope.$parent.productId, false);
            }
        };

        vm.resetDataModel = function (accessType) {
            if (accessType === 'propertyGroup') {
                syncMgr.allPropertiesSync($scope.$parent.productId, false);
                syncMgr.updateProductAllProperties($scope.$parent.productId, false);
            }
            else if(accessType === 'property') {
                syncMgr.setAllPropertyGroupSync($scope.$parent.productId, false);
                syncMgr.updateProductAllProperties($scope.$parent.productId, false);
            }
            else if(accessType === 'allProperties') {
                syncMgr.allPropertiesSync($scope.$parent.productId, false);
                syncMgr.updateProductAllProperties($scope.$parent.productId, true);
            }
            vm.propertySelect = accessType;
            if($scope.$parent.productId !== 8){
                syncMgr.setAccessTypeValue($scope.$parent.productId, accessType);
            }
            var dependencyControlId = syncMgr.getProductDependencyControlId($scope.$parent.productId, accessType);
            if (dependencyControlId > 0) {
                vm.loadProductControlDependencyData(dependencyControlId);
            }
            pubsub.publish("ppanel.access-type-change", accessType);
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
            if (productId == 17 || productId == 26 || productId == 18 || productId == 47) {
                vm.rpRoleSelected = roleData.find(function (item) {
                    return item.isAssigned === true;
                });
                logc("vm.rpRoleSelected", vm.rpRoleSelected);
            }

            if (productId == 16 && vm.propertySelect !== undefined) {
                vm.rpRoleSelected = vm.propertySelect;
            }
            var presetroleData = syncMgr.getProductPresetRolesData(productId);

            if (roleData && roleData.length > 0) {
                roleData.forEach(function (item) {
                    angular.extend(item, {
                        disableSelection: vm.hasViewOnlyAccess(),
                        radname: "role",
                        productId: productId
                    });

                    if (item.isAssigned && item.userRights !== undefined) {
                        vm.assignedRoleId = item.roleId;
                        vm.roleRights = item.userRights;
                    }

                    if (productId == 20 && item.roletype === undefined) {
                        angular.extend(item, {
                            roletype: item.name
                        });
                    }
                });

                var controlDependencyValue = (productId === 16 && vm.propertySelect == '') ? 'property' : 'role';
                var dependencyControlId = syncMgr.getProductDependencyControlId(productId, controlDependencyValue);

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

                    vm.selectconfigs.forEach(function (item) {
                        item.configData.setOptions(vm.presetRoles);
                    });

                }

                roleGridPagination.setData(roleData).goToPage({
                    number: 0
                });
            }

            return vm;
        };

        vm.roleSelectedChange = function (val) {
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

        vm.configAPanelbyRoleTypes = function (rolesList){
            var distinctRoles = [];
            var roletype = "";
            rolesList.forEach(function (role) {
                if(role.roletype !== undefined){
                    roletype = role.roletype.replace(/ /g, "");
                    if(distinctRoles.indexOf(roletype) == -1){
                        distinctRoles.push(roletype);
                    }
                }
            });
            pubsub.publish("ppanel.distinct-role-types", distinctRoles);
        };
        vm.setRolesData = function (resp) {
            rolesGrid.busy(false);
            if (resp.records && resp.records.length > 0) {
                if ($scope.$parent.productId == 47) {
                    var userPersonaId = userDetailsModel.getPersonaId();
                    if (userPersonaId === 0) {
                        resp.records.forEach(function (role) {
                            if (role.id === 'agent') {
                                role.isAssigned = true;
                            }
                        });
                    }
                    else {

                    }

                    var reportValue = userPersonaId === 0 ? true : resp.additional.canReceiveMonthlyReport;
                    syncMgr.setCanReceiveMonthlyReport(reportValue);
                }

                var rdata = syncMgr.setRoleList(resp.records, $scope.$parent.productId);
                if (resp.additional && resp.additional.presets) {
                    syncMgr.setPresetRoleList(resp.additional.presets, $scope.$parent.productId);
                }

                if ($scope.$parent.productId == 20) {
                    var totalCount = 0;
                    var roleList = resp.records;
                    vm.configAPanelbyRoleTypes(roleList);
                    roleList.forEach( function(record){
                        if(record.propertiesList !== null){
                            var propertyList = record.propertiesList;
                            var assignedPropertiesCount = propertyList.filter(function (prop) {
                                return prop.isAssigned === true;
                            });
                            record.assignedProperties = assignedPropertiesCount.length + " of " + propertyList.length;
                        }
                    });
                    
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
                    item.configData = menuConfig({
                        nameKey: "name",
                        valueKey: "id",
                        fieldName: "roleSelect",
                        onChange: vm.roleSelectedChange,
                        disabled: vm.hasViewOnlyAccess()
                    });
                    vm.selectconfigs.push(item);
                });
            }
        };

        vm.setControlDependencyData = function (resp) {
            var productId = $scope.$parent.productId;
            var tabs = syncMgr.getProductInitialTabs(productId);

            if (resp.data && resp.data.length > 0) {
                var matchFound = false;
                if (productId == 3) {
                    vm.roleRights.forEach(function (right) {
                        if (right.rightNickName) {
                            var record = resp.data.filter(function (data) {
                                return right.rightNickName.toLowerCase() === data.masterControlValue.toLowerCase();
                            })[0];

                            if (record !== undefined && record) {
                                matchFound = true;
                            }
                        }
                    });
                }
                else if (productId == 16) {
                    var irreleventTab = resp.data[0];
                    var relevantTab = [];
                    var allTab = syncMgr.getProductAllTabs($scope.$parent.productId);

                    relevantTab = allTab.filter(function (data) {
                        return data.text.toLowerCase() !== irreleventTab.displayName.toLowerCase();
                    });
                    if (vm.propertySelect == '' || vm.propertySelect === undefined) {
                        relevantTab = relevantTab.filter(function (data) {
                            return data.id.toLowerCase() !== 'properties';
                        });
                    }
                    vm.setProductTabs(relevantTab);
                    matchFound = true;
                }
                else if (productId == 17 || productId == 26 || productId == 18 || productId == 47) {
                    var releventTabs = [];
                    var rpTabs = [];
                    var rpRoleName = "";

                    if (vm.rpRoleSelected) {
                        rpRoleName = vm.rpRoleSelected.name.toLowerCase();
                        var tabsData = $filter("orderBy")(resp.data, "slaveControlId");
                        releventTabs = tabsData.filter(function (data) {
                            return data.masterControlValue.toLowerCase() == rpRoleName;
                        });

                        if(productId == 17 && vm.rpRoleSelected.name.toLowerCase() == "enterprise standard"){
                            vm.showAllPropertiesSwitch = true;
                            vm.allProperties = syncMgr.isProductAllProperties(productId);
                        }
                    }

                    if (releventTabs.length > 0) {
                        var allTabs = syncMgr.getProductAllTabs($scope.$parent.productId);
                        logc("allTabs", releventTabs, allTabs, $scope.$parent.productId);
                        // if (productId == 47) {
                        //     var roleTab = allTabs.find(function (item) {
                        //         return item.text === "Roles";
                        //     });
                        //     rpTabs.push(roleTab);
                        // }
                        releventTabs.forEach(function (tb) {
                            var rpTab = allTabs.find(function (item) {
                                return item.text === tb.displayName;
                            });
                            if (rpTab != undefined) {
                                rpTabs.push(rpTab);
                            }
                        });

                        if(productId == 17 && syncMgr.isProductAllProperties(productId)){
                            var filterTabs = rpTabs.find(function(tb){
                                return tb.text == "Roles";
                            });
                            rpTabs = [filterTabs];
                        }

                        vm.setProductTabs(rpTabs);
                        matchFound = true;
                    }
                    else {
                        vm.setProductTabs(tabs);
                    }

                    if (productId == 26) {
                        if (rpTabs.length > 0) {
                            vm.setAllProperties(rpTabs);
                        }
                        else {
                            syncMgr.updateProductAllProperties(productId, false);
                        }
                    }
                    if (productId == 18 && vm.rpRoleSelected) {
                        vm.setAllProperties(vm.rpRoleSelected);
                    }
                }

                //Exclude properties tab fro employee and external user company
                var compId = persona.getBooksMasterId();
                if (compId === -1 || compId === -2) {
                    matchFound = false;
                    vm.setProductTabs(tabs);

                }
                else if (productId == 3) {
                    if (matchFound) {
                        tabs = syncMgr.getProductAllTabs($scope.$parent.productId);
                        vm.setProductTabs(tabs);
                    }
                    else {
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
            vm.isSelectAllPropMsg2 = false;
            if (record.productId == "3" || record.productId == "17" || record.productId == "18" || record.productId == "26" || record.productId == "47") {
                var dependencyControlId = syncMgr.getProductDependencyControlId(record.productId, record.radname);
                if (record.productId == "17") {
                    var existingRole = vm.rpRoleSelected.name ?  vm.rpRoleSelected.name : "";
                    vm.showAllPropertiesSwitch = (record.name.toLowerCase() == "enterprise standard") ? true : false;
                    var isAllProperties = (record.name.toLowerCase() == "enterprise admin") ? true : false;
                    syncMgr.updateProductAllProperties($scope.$parent.productId, isAllProperties);
                    
                    if((vm.allProperties && record.name.toLowerCase().indexOf('staff') !== -1 && existingRole.toLowerCase().indexOf('enterprise') !== -1) ||(existingRole.toLowerCase() == "enterprise admin")) {
                        syncMgr.allPropertiesSync(record.productId, false);
                        vm.allProperties = false;
                    }
                    vm.rpRoleSelected = record;
                }
                else if (record.productId == "26" || record.productId == "47") {
                    vm.rpRoleSelected = record;
                }
                else if (record.productId == "18") {
                    if (vm.rpRoleSelected) {
                        if (record.name.toLowerCase() === "property manager") {
                            syncMgr.setAllPropertyGroupSync(record.productId, false);
                        }
                        else if (record.name.toLowerCase() === "group manager") {
                            syncMgr.allPropertiesSync(record.productId, false);
                        }
                        else {
                            syncMgr.setAllPropertyGroupSync(record.productId, false);
                            syncMgr.allPropertiesSync(record.productId, false);
                        }
                    }
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
                if (dependencyControlId > 0) {
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
            if (productId == 17) {
                var tabs = syncMgr.getProductInitialTabs($scope.$parent.productId);
                var releventTabs = [];
                if (record) {
                    releventTabs = tabs.filter(function (data) {
                        return data.text.toLowerCase() == "roles";
                    });
                }
                else{
                    syncMgr.allPropertiesSync($scope.$parent.productId, false);
                    pubsub.publish("rp.residentPortalAllPropertiesSet","property");
                }

                if(releventTabs != undefined && releventTabs.length >0){
                    vm.setProductTabs(releventTabs);
                }
                else {
                    vm.setProductTabs(tabs);
                }
                vm.allProperties = record;
                syncMgr.updateProductAllProperties($scope.$parent.productId, record);
            }
            else if (productId == 26 && record.length == 1 && record[0].text == "Roles") {
                vm.isSelectAllPropMsg1 = true;
                syncMgr.updateProductAllProperties($scope.$parent.productId, true);
            }
            else if (productId == 18) {
                if (record.name.toLowerCase() === "portfolio manager") {
                    vm.isSelectAllPropMsg2 = true;
                    syncMgr.updateProductAllProperties($scope.$parent.productId, true);
                }
                else {
                    syncMgr.updateProductAllProperties($scope.$parent.productId, false);

                }
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
            vm.showAllPropertiesSwitch = false;
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

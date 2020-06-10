//  Sync Manager Model

(function (angular, undefined) {
    "use strict";

    function factory(pubsub, security, persona) {
        function ProductDataSyncManager() {
            var s = this;
            s.init();
        }

        var p = ProductDataSyncManager.prototype;

        p.init = function () {
            var s = this;
            s.gridConfigLoaded = false;
            s.switchConfigLoaded = false;
            s.selectTypeConfigLoaded = false;
            s.productControlsMap = {};
            s.propertyGroupMap = {};
            s.propertyMap = {};
            s.roleMap = {};
            s.benchMarkRoleMap = {};
            s.tab6DataMap = {};
            s.rightMap = {};
            s.sidePanelDataMap = {};
            s.productGridConfigMap = {};
            s.productAsideGridConfigMap = {};
            s.productSwitchConfigMap = {};
            s.productRadioConfigMap = {};
            s.productDependencyMap = {};
            s.productTabsMap = {};
            s.productActiveTabMap = {};
            s.productSelectTypeConfigMap = {};
            s.productPageLevelRadioConfigMap = {};
            s.productDependencyDataMap = {};
            s.productPresetRolesMap = {};
            s.notificationsMap={};
            s.originalPropertyListMap = {};
            s.asidePropertyMap = {};

            s.productControlsList = {
                products: []
            };
            //s.productControlsList = [];
            s.groupList = [];
            s.propertyList = [];
            s.propertyGroupList = [];
            s.productsTouched = [];
            s.originalPropertyList = [];
            s.roleList = [];
            s.benchMarkRoleList = [];
            s.tab6DataList = [];
            s.rightList = [];
            s.presetRoleList = [];
            s.sidePanelDataList = [];
            s.asidePropertyList = [];

        };

        // Getters

        p.getSelectedCount = function (list, selectKey) {
            var s = this,
                count = 0;

            list.forEach(function (item) {
                if (item[selectKey]) {
                    count++;
                }
            });

            return count;
        };

        // p.getProductsTouched = function () {
        //     var s = this;
        //     return s.productsTouched;
        // };

        p.getProductControlsList = function () {
            var s = this;
            return s.productControlsList;
        };

        p.getPropertyList = function () {
            var s = this;
            return s.propertyList;
        };

        p.getOriginalPropertyList = function () {
            var s = this;
            return s.originalPropertyList;
        };

        p.getRoleList = function () {
            var s = this;
            return s.roleList;
        };

        p.getProductGridConfig = function (productId, tabName) {
            var s = this,
                gridConfig;
            if (s.productGridConfigMap['product' + productId + tabName] !== undefined) {
                gridConfig = s.productGridConfigMap['product' + productId + tabName].gridConfig[0];
            }
            return gridConfig;
        };

        p.getProductAsideGridConfig = function (productId, tabName) {
            var s = this,
                config;
            if (s.productAsideGridConfigMap['product' + productId + tabName] !== undefined) {
                config = s.productAsideGridConfigMap['product' + productId + tabName].asideGridConfig[0];
            }
            return config;
        };

        p.getProductAsideGridName = function (productId, tabName) {
            var s = this,
                name;
            if (s.productAsideGridConfigMap['product' + productId + tabName] !== undefined) {
                name = s.productAsideGridConfigMap['product' + productId + tabName].displayName;
            }
            return name;
        };

        p.getProductSwitchConfig = function (productId, tabName) {
            var s = this,
                config;
            if (s.productSwitchConfigMap['product' + productId + tabName] !== undefined) {
                config = s.productSwitchConfigMap['product' + productId + tabName].switchCtrlConfig;
            }
            return config;
        };

        p.getProductSelectTypeConfig = function (productId, tabName) {
            var s = this,
                config;
            if (s.productSelectTypeConfigMap['product' + productId + tabName] !== undefined) {
                config = s.productSelectTypeConfigMap['product' + productId + tabName].selectCtrlConfig;
            }
            return config;
        };

        p.getProductPageLevelRadioConfig = function (productId, tabName) {
            var s = this,
                config;
            if (s.productPageLevelRadioConfigMap['product' + productId + tabName] !== undefined) {
                config = s.productPageLevelRadioConfigMap['product' + productId + tabName].selectCtrlConfig;
            }
            return config;
        };

        p.getProductRadioConfig = function (productId, tabName) {
            var s = this,
                config;
            if (s.productRadioConfigMap['product' + productId + tabName] !== undefined) {
                config = s.productRadioConfigMap['product' + productId + tabName].radioConfig;
            }
            return config;
        };

        p.getPageDisplayName = function (product) {
            var s = this,
                pageDisplayName = "";
            if (s.productControlsMap['product' + product] !== undefined) {
                pageDisplayName = s.productControlsMap['product' + product].displayName;
            }

            return pageDisplayName;
        };

        p.getProductControls = function (product) {
            var s = this,
                productControlList;

            if (s.productControlsMap['product' + product] !== undefined) {
                logc(product, s.productControlsMap['product' + product]);
                productControlList = s.productControlsMap['product' + product].control;
            }
            logc("productControlList", productControlList);
            return productControlList;
        };

        p.getProductRolesData = function (product) {
            var s = this,
                productRolesList;

            if (s.roleMap['product' + product] !== undefined) {
                productRolesList = s.roleMap['product' + product].roles;
            }

            return productRolesList;
        };

        p.getProductBenchMarkRolesData = function (product) {
            var s = this,
                productBMRolesList;

            if (s.benchMarkRoleMap['product' + product] !== undefined) {
                productBMRolesList = s.benchMarkRoleMap['product' + product].roles;
            }

            return productBMRolesList;
        };

        p.getTab6ProductData = function (product) {
            var s = this,
                productDataList;

            if (s.tab6DataMap['product' + product] !== undefined) {
                productDataList = s.tab6DataMap['product' + product].data;
            }

            return productDataList;
        };

        p.getProductRightsData = function (product) {
            var s = this,
                productRightsList;

            if (s.rightMap['product' + product] !== undefined) {
                productRightsList = s.rightMap['product' + product].rights;
            }

            return productRightsList;
        };

        p.getProductPresetRolesData = function (product) {
            var s = this,
                productRolesList;

            if (s.productPresetRolesMap['product' + product] !== undefined) {
                productRolesList = s.productPresetRolesMap['product' + product].roles;
            }
            // logc("master data",product,s.roleMap, productRolesList);
            return productRolesList;
        };

        p.getProductNotificationsData = function (product) {
            var s = this,
                productNotificationsList;
            if (s.notificationsMap['product' + product] !== undefined) {
                productNotificationsList = s.notificationsMap['product' + product].notifications;
            }
            return productNotificationsList;
        };

        p.getProductPropertiesData = function (product) {
            var s = this,
                productPropertiesList;
            //logc(s.propertyMap['product' + product]);
            if (s.propertyMap['product' + product] !== undefined) {
                //logc(s.propertyMap['product' + product]);
                productPropertiesList = s.propertyMap['product' + product].properties;
            }
            //logc("master data",product,s.propertyMap, productPropertiesList);
            return productPropertiesList;
        };
        p.getProductAsidePropertyData = function (product) {
            var s = this,
                asidePropertyList;

            if (s.asidePropertyMap['product' + product] !== undefined) {
                asidePropertyList = s.asidePropertyMap['product' + product].asideProperties;
            }
            return asidePropertyList;
        };

        p.getProductPropertyGroupData = function (product) {
            var s = this,
                productPropertyGroupList;

            if (s.propertyGroupMap['product' + product] !== undefined) {
                productPropertyGroupList = s.propertyGroupMap['product' + product].propertyGroup;
            }

            return productPropertyGroupList;
        };

        p.getProductDependencyControlId = function (product, name) {
            var s = this,
                controlId = 0;

            if (s.productDependencyMap['product' + product + name] !== undefined) {
                controlId = s.productDependencyMap['product' + product + name].controlId;
            }
            //logc("master data",product,s.propertyMap, productPropertiesList);
            return controlId;
        };


        p.getProductAllTabs = function (product) {
            var s = this,
                productTabsList;

            if (s.productTabsMap['product' + product] !== undefined) {
                productTabsList = s.productTabsMap['product' + product].allTabs;
            }
            //logc("master data",product,s.propertyMap, productPropertiesList);
            return productTabsList;
        };

        p.getProductInitialTabs = function (product) {
            var s = this,
                productTabsList;

            if (s.productTabsMap['product' + product] !== undefined) {
                productTabsList = s.productTabsMap['product' + product].intialTabs;
            }
            //logc("master data",product,s.propertyMap, productPropertiesList);
            return productTabsList;
        };

        p.getProductActiveTab = function (product) {
            var s = this,
                activeTab;

            if (s.productActiveTabMap['product' + product] !== undefined) {
                activeTab = s.productActiveTabMap['product' + product].activeTab;
            }
            //logc("master data",product,s.propertyMap, productPropertiesList);
            return activeTab;
        };
        // Setters


        p.setPropertyList = function (list, key) {
            var s = this;
            s.propertyList = list;
            s.renderPropertyMap(key);
            return s;
        };

        p.setAsidePropertyList = function (list, key) {
            var s = this;
            s.asidePropertyList = list;
            s.renderAsidePropertyMap(key);
            return s;
        };

        p.setPropertyGroupList = function (list, key) {
            var s = this;
            s.propertyGroupList = list;
            s.renderPropertyGroupMap(key);
            return s;
        };

        p.setOriginalPropertyList = function (list) {
            var s = this;
            s.originalPropertyList = angular.copy(list);
            return s;
        };

        p.setPropertySelectKey = function (key) {
            var s = this;
            s.propertySelectKey = key;
            return s;
        };

        p.setProductDataSelectKey = function (key) {
            var s = this;
            s.productDataSelectKey = key;
            return s;
        };

        p.setProductControlsList = function (list) {
            var s = this;
            s.productControlsList.products.push(list);
            s.renderProductControlsMap();
            return s;
        };

        p.setRoleList = function (list, key) {
            var s = this;
            s.roleList = list;
            s.renderRoleMap(key);
            return s;
        };

        p.setBenchMarkRoleList = function (list, key) {
            var s = this;
            s.benchMarkRoleList = list;
            s.renderBenchMarkRoleMap(key);
            return s;
        };

         p.setTab6DataList = function (list, key) {
            var s = this;
            s.tab6DataList = list;
            s.renderTab6DataMap(key);
            return s;
        };

        p.setRightList = function (list, key) {
            var s = this;
            s.rightList = list;
            s.renderRightMap(key);
            return s;
        };
        p.setPresetRoleList = function (list, key) {
            var s = this;
            s.presetRoleList = list;
            s.renderPresetRoleMap(key);
            return s;
        };

        p.setRoleSelectKey = function (key) {
            var s = this;
            s.roleSelectKey = key;
            return s;
        };

        p.setProductDependencyDataMap = function (key, bool) {
            var s = this;

            s.productDependencyDataMap['product' + key] = {
                dependencyData: bool
            };
        };

        p.setProductAllNotifications = function (product, value) {
            var s = this;
            s.notificationsMap['product' + product] = {
                notifications: value
            };
        };

        p.updateProductAllProperties = function (product, value) {
            var s = this,
                productPropertiesList;

            if (s.propertyMap['product' + product] !== undefined) {
                s.propertyMap['product' + product].allProperties = value;
            }
        };

        p.updateProductNewPropertyByDefault = function (product, value) {
            var s = this,
                productPropertiesList;

            if (s.propertyMap['product' + product] !== undefined) {
                s.propertyMap['product' + product].newPropertyByDefault = value;
            }
            logc("sysncdata", s);
        };

        p.isProductAllProperties = function (productId) {
            var s = this;

            if (s.propertyMap['product' + productId] !== undefined &&
                productId !== 9) {
                return s.propertyMap['product' + productId].allProperties;
            }

            return false;
        };

        p.isProductNewPropertyByDefault = function (product) {
            var s = this;

            if (s.propertyMap['product' + product] !== undefined) {
                return s.propertyMap['product' + product].newPropertyByDefault;
            }

            return false;
        };

        p.isSwitchConfigLoaded = function () {
            var s = this;
            return s.switchConfigLoaded;
        };

        p.isSelectTypeConfigLoaded = function () {
            var s = this;
            return s.selectTypeConfigLoaded;
        };

        p.isProductDependencyDataNeeded = function (product) {
            var s = this,
                dependencyData = false;

            if (s.productDependencyDataMap['product' + product] !== undefined) {
                dependencyData = s.productDependencyDataMap['product' + product].dependencyData;
            }
            //logc("master data",product,s.propertyMap, productPropertiesList);
            return dependencyData;
        };

        p.selectedRoleSync = function (key, record) {
            var s = this,
                roleData,
                selectedRole,
                selectState = false;

            roleData = s.roleMap['product' + key].roles;

            roleData.forEach(function (item) {
                item.isAssigned = false;
                item.isAssigned = item.id == record.id;
            });

            return s;
        };

        p.selectedBenchMarkRoleSync = function (key, record) {
            var s = this,
                roleData,
                selectedRole,
                selectState = false;

            roleData = s.benchMarkRoleMap['product' + key].roles;

            roleData.forEach(function (item) {
                item.isAssigned = false;
                item.isAssigned = item.id == record.id;
            });

            return s;
        };

         p.selectedTab6DataSync = function (key, record) {
            var s = this,
                tabData,
                selectedRole,
                selectState = false;

            tabData = s.tab6DataMap['product' + key].data;

            tabData.forEach(function (item) {
                item.isAssigned = false;
                item.isAssigned = item.id == record.id;
            });

            return s;
        };

        p.selectedRightSync = function (key, record) {
            var s = this,
                rightData,
                selectedRole,
                selectState = false;

            rightData = s.rightMap['product' + key].rights;

            rightData.forEach(function (item) {
                item.isAssigned = false;
                item.isAssigned = item.id == record.id;
            });

            return s;
        };

        p.multiSelectedRoleSync = function (key, record) {
            var s = this,
                roleData,
                selectedRole,
                selectState = false;

            roleData = s.roleMap['product' + key].roles;

            roleData.forEach(function (item) {

                if (item.id == record.id) {
                    item.isAssigned = record.isAssigned;
                }

                if (record.productId == 20 &&
                    item.roletype === record.roletype &&
                    item.id !== record.id)
                {
                    item.disableSelection = record.isAssigned;
                }
            });

            return s;
        };

        p.multiSelectBenchMarkRoleSync = function (key, record) {
            var s = this,
                roleData,
                selectedRole,
                selectState = false;

            roleData = s.benchMarkRoleMap['product' + key].roles;
            roleData.forEach(function (item) {
                if (item.id == record.id) {
                    item.isAssigned = record.isAssigned;
                }
            });

            return s;
        };

        p.multiSelectTab6DataSync = function (key, record) {
            var s = this,
                tabData,
                selectedRole,
                selectState = false;

            tabData = s.tab6DataMap['product' + key].data;
            tabData.forEach(function (item) {
                if (item.id == record.id) {
                    item.isAssigned = record.isAssigned;
                }
            });

            return s;
        };

        p.multiSelectRightSync = function (key, record) {
            var s = this,
                rightData,
                selectedRight,
                selectState = false;
            rightData = s.rightMap['product' + key].rights;
            rightData.forEach(function (item) {
                if (item.id == record.id) {
                    item.isAssigned = record.isAssigned;
                }
            });

            return s;
        };

        p.setSelectedPresetRoleSync = function (key, list) {
            var s = this,
                roleData,
                selectedRole,
                selectState = false;

            roleData = s.roleMap['product' + key].roles;

            roleData.forEach(function (item) {
                item.isAssigned = false;
                if (list.contains(parseInt(item.id))) {
                    item.isAssigned = true;
                }
            });

            return s;
        };

        p.selectedPropertySync = function (key, record) {
            var s = this,
                propertyData;

            propertyData = s.propertyMap['product' + key].properties;

            propertyData.forEach(function (item) {
                item.isAssigned = false;
                item.isAssigned = item.id == record.id;
            });

            return s;
        };
        p.selectedAsidePropertySync = function (productId) {
            var s = this,
            propertyData;

            propertyData = s.asidePropertyMap['product' + productId].asideProperties;
            var assignedPropertiesCount = propertyData.propertiesList.filter(function (data) {
                return data.isAssigned === true;
            });

            propertyData.assignedProperties = assignedPropertiesCount.length+" of "+ propertyData.propertiesList.length;
            return s;
        };

        p.multiSelectedPropertySync = function (key, record) {
            var s = this,
                propertyData;

            propertyData = s.propertyMap['product' + key].properties;

            propertyData.forEach(function (item) {
                if (item.id == record.id) {
                    item.isAssigned = record.isAssigned;
                }

            });

            return s;
        };

        p.updateAllProperties = function (key, records) {
            var s = this,
                propertyData;

            propertyData = s.propertyMap['product' + key].properties;

            records.forEach(function (item) {
                var record = propertyData.filter(function (data) {
                    return item.id === data.id;
                })[0];

                if (item.id == record.id) {
                    record.isAssigned = item.isAssigned;
                }

            });

            return s;
        };

        p.groupToPropertySync = function (productId, group) {
            var s = this,
                propertyList,
                selectState = false;

            propertyList = s.propertyMap['product' + productId].properties;

            propertyList.forEach(function (item) {
                if (parseInt(item.region_id) == parseInt(group.id)) {
                    item["isAssigned"] = group.isAssigned;
                }
            });

            pubsub.publish("pplpropertygroup.updateGrids");

            return s;
        };

        p.allPropertiesSync = function (productId, selected) {
            var s = this,
                propertyList,
                selectState = false,
                assignedCount = 0,
                totalCount = 0;

            propertyList = s.propertyMap['product' + productId].properties;
            propertyList.forEach(function (item) {
                item["isAssigned"] = selected;
                if (item.isAssigned) {
                    assignedCount++;
                }
                totalCount++;
            });

            propertyList.assignedProperties = assignedCount + " of " + totalCount;
            pubsub.publish("pplpropertygroup.updateGrids");
            return s;
        };
        p.updateAllFilterAsideProperties = function (productId, record, bool) {
            var s = this,
                propertyList,
                assignedCount = 0,
                matchRecord;

            propertyList = s.asidePropertyMap['product' + productId].asideProperties;
            record.forEach(function (item) {
                item.isAssigned = bool;
            });
            if(bool){
                assignedCount = record.length;
            }
            propertyList.assignedProperties = assignedCount + " of " + propertyList.propertiesList.length;
            return s;
        };
        p.setAllPropertyGroupSync = function (productId, bool) {
            var s = this,
                propertyGroupList;
            if(!angular.equals({}, s.propertyGroupMap)){
                propertyGroupList = s.propertyGroupMap['product' + productId].propertyGroup;
                propertyGroupList.forEach(function (item) {
                    item["isAssigned"] = bool;
                });
            }

            return s;
        };

        p.allRolesSync = function (productId, selected) {
            var s = this,
                roleList,
                selectState = false,
                assignedCount = 0,
                totalCount = 0;

            roleList = s.roleMap['product' + productId].roles;

            roleList.forEach(function (item) {
                item["isAssigned"] = selected;
                if (item.isAssigned) {
                    assignedCount++;
                }
                totalCount++;
            });

            roleList.assignedRoles = assignedCount + " of " + totalCount;
            return s;
        };

        p.allBenchMarkRolesSync = function (productId, selected) {
            var s = this,
                roleList,
                selectState = false,
                assignedCount = 0,
                totalCount = 0;

            roleList = s.benchMarkRoleMap['product' + productId].roles;

            roleList.forEach(function (item) {
                item["isAssigned"] = selected;
            });

            return s;
        };

         p.allTab6DataSync = function (productId, selected) {
            var s = this,
                list,
                selectState = false,
                assignedCount = 0,
                totalCount = 0;

            list = s.tab6DataMap['product' + productId].data;

            list.forEach(function (item) {
                item["isAssigned"] = selected;
            });

            return s;
        };

        p.allRightsSync = function (productId, selected) {
            var s = this,
                rightList,
                selectState = false,
                assignedCount = 0,
                totalCount = 0;

            rightList = s.rightMap['product' + productId].rights;

            rightList.forEach(function (item) {
                item["isAssigned"] = selected;
            });

            return s;
        };
        p.updateAllFilterRights = function (key, records, bool) {
            var s = this,
            rightData;
            rightData = s.rightMap['product' + key].rights;
            records.forEach(function (item) {
                var record = rightData.filter(function (data) {
                    return item.id === data.id;
                })[0];

                if (item.id == record.id) {
                    record.isAssigned =bool;
                }
            });
            return s;
        };

        p.updatePropertyList = function (property) {
            var s = this;
            s.propertyList.push(property);
            return s;
        };

        p.getCompanyGroupList = function () {
            var s = this;
            return s.companyGroupList;
        };

        p.renderProductGridConfigMap = function (productId, tabName, config) {
            var s = this;
            s.productGridConfigMap['product' + productId + tabName] = {
                gridConfig: config
            };
        };

        p.renderProductAsideGridConfigMap = function (productId, tabName, config, name) {
            var s = this;
            s.productAsideGridConfigMap['product' + productId + tabName] = {
                asideGridConfig: config,
                displayName: name
            };
        };

        p.renderProductSwitchConfigMap = function (productId, tabName, config) {
            var s = this;
            //logc("switchConfig", config);
            s.productSwitchConfigMap['product' + productId + tabName] = {
                switchCtrlConfig: config
            };
            s.switchConfigLoaded = true;
            // logc("s.productSwitchConfigMap", s.productSwitchConfigMap);
        };

        p.renderProductSelectTypeConfigMap = function (productId, tabName, config) {
            var s = this;
            s.productSelectTypeConfigMap['product' + productId + tabName] = {
                selectCtrlConfig: config
            };

            s.selectTypeConfigLoaded = true;
        };

        p.renderPageLevelRadioConfigMap = function (productId, tabName, config) {
            var s = this;
            s.productPageLevelRadioConfigMap['product' + productId + tabName] = {
                selectCtrlConfig: config
            };
        };

        p.renderProductRadioConfigMap = function (productId, tabName, config) {
            var s = this;
            s.productRadioConfigMap['product' + productId + tabName] = {
                radioConfig: config
            };
        };

        p.renderProductControlsMap = function () {
            var s = this;

            if (!angular.equals({}, s.productControlsList)) {
                s.productControlsList.products.forEach(function (product) {
                    s.productControlsMap['product' + product.productId] = {
                        control: product,
                        displayName: product.pageDisplayName,
                        controls: []
                    };
                });
            }
        };

        p.renderProductDependencyMap = function (key, name, controlId) {
            var s = this;

            s.productDependencyMap['product' + key + name] = {
                controlId: controlId
            };
        };

        p.renderProductTabsMap = function (key, allTabs, initTab) {
            var s = this;

            s.productTabsMap['product' + key] = {
                allTabs: allTabs,
                intialTabs: initTab
            };
        };

        p.renderProductActiveTabMap = function (key, name) {
            var s = this;

            s.productActiveTabMap['product' + key] = {
                activeTab: name
            };
        };

        p.renderPropertyMap = function (key) {
            var s = this;
            if (!angular.equals({}, s.propertyList)) {
                s.propertyMap['product' + key] = {
                    properties: s.propertyList,
                    allProperties: false,
                    newPropertyByDefault: false
                };
            }
        };
        p.renderAsidePropertyMap = function (key) {
            var s = this;
            if (!angular.equals({}, s.asidePropertyList)) {
                s.asidePropertyMap['product' + key] = {
                    asideProperties: s.asidePropertyList,
                };
            }
        };

        p.renderPropertyGroupMap = function (key) {
            var s = this;
            if (!angular.equals({}, s.propertyGroupList)) {
                s.propertyGroupMap['product' + key] = {
                    propertyGroup: s.propertyGroupList
                };
            }
        };

        p.renderOriginalPropertyListMap = function (key) {
            var s = this,
                assignedProp = [];

            if (!angular.equals({}, s.propertyList)) {
                s.propertyList.forEach(function (item) {
                    if (item.isAssigned) {
                        assignedProp.push(item.id);
                    }
                });

                s.originalPropertyListMap['product' + key] = {
                    properties: s.assignedProp
                };
            }
        };

        p.renderRoleMap = function (key) {
            var s = this;

            if (!angular.equals({}, s.roleList)) {
                s.roleMap['product' + key] = {
                    roles: s.roleList
                };
            }
        };

        p.renderBenchMarkRoleMap = function (key) {
            var s = this;

            if (!angular.equals({}, s.benchMarkRoleList)) {
                s.benchMarkRoleMap['product' + key] = {
                    roles: s.benchMarkRoleList
                };
            }
        };

        p.renderTab6DataMap = function (key) {
            var s = this;

            if (!angular.equals({}, s.tab6DataList)) {
                s.tab6DataMap['product' + key] = {
                    data: s.tab6DataList
                };
            }
        };

        p.renderRightMap = function (key) {
            var s = this;

            if (!angular.equals({}, s.rightList)) {
                s.rightMap['product' + key] = {
                    rights: s.rightList
                };
            }
        };
        p.renderPresetRoleMap = function (key) {
            var s = this;

            if (!angular.equals({}, s.presetRoleList)) {
                s.productPresetRolesMap['product' + key] = {
                    roles: s.presetRoleList
                };
            }
        };

        p.renderMap = function (key) {
            var s = this;

            if (!s.roleList.empty()) {
                s.roleList.forEach(function (role) {
                    s.roleMap['company' + role.companyId] = {
                        role: role.roles,
                        roles: []
                    };
                });
            }


            if (!s.propertyList.empty()) {
                s.propertyList.forEach(function (property) {
                    s.propertyMap['company' + property.companyId] = {
                        property: property,
                        properties: []
                    };
                });
            }

        };

        p.updateSelectState = function (list, selectKey, bool) {
            var s = this;

            list.forEach(function (item) {
                item[selectKey] = bool;
            });

            return s;
        };

        p.clearPropertyGroupData = function(key) {
            var s = this;
            var list = s.propertyGroupMap['product' + key].propertyGroup;
            list.forEach(function (item) {
                item.isAssigned = false;
            });
            s.propertyGroupList = list;
            s.renderPropertyGroupMap(key);
        };

        // Assertions

        p.allSelected = function (list, selectKey) {
            var s = this;
            return s.getSelectedCount(list) === list.length;
        };

        // p.isUserHasManageProductAccess = function () {
        //     return !persona.data.hasManageAssetOptimizationProductAccess;
        // };
        p.isUserHasManageProductAccess = function (productId) {
            //var productId = $scope.$parent.productId;
            //logc("test", persona.data.hasProspectContactCenterProductAccess);
            var s = this;
            switch (productId) {
                case "1":
                    return persona.data.hasManageOneSiteProductAccess;
                case "4":
                    return persona.data.hasManageAssetOptimizationProductAccess;
                case "6":
                    return persona.data.hasManageLead2LeaseProductAccess;
                case "8":
                    return persona.data.hasManageAccountingProductAccess;
                case "9":
                    return persona.data.hasManageMarketingCenterProductAccess;
                case "10":
                    return persona.data.hasProspectContactCenterProductAccess;
                case "13":
                    return persona.data.hasManageSpendManagementProductAccess;
                case "14":
                    return persona.data.hasManageClientPortalProductAccess;
                case "15":
                    return persona.data.hasManageRentersInsuranceProductAccess;
                case "16":
                    return persona.data.hasManageVendorComplianceProductAccess;
                case "17":
                    return persona.data.hasResidentPortalUserAccess;
                case "18":
                    return persona.data.hasManageUtilityManagementProductAccess;
                case "20":
                    return persona.data.hasManageDocumentManagementProductAccess;
                case "23":
                    return persona.data.hasManageOnSiteProductAccess;
                case "26":
                    return persona.data.hasManageUnifiedAmenitiesProductAccess;
                case "39":
                    return persona.data.hasManageIntegrationMarketplaceProductAccess;
                case "40":
                    return persona.data.hasManageILMLeadManagemementProductAccess;
                case "41":
                    return persona.data.hasManageILMLeasingAnalyticsProductAccess;
                case "44":
                    return persona.data.hasManagePortfolioManagementProductAccess;
                case "47":
                    return persona.data.hasManageDepositAlternativeProductAccess;
                case "48":
                    return persona.data.hasManageClickPayProductAccess;
                default:
                    return false;
            }
        };

        p.reset = function () {
            var s = this;
            s.propertyGroupMap = {};
            s.companyGroupMap = {};
            s.propertyMap = {};
            s.roleMap = {};
            s.rightMap = {};
            s.bmRoleMap = {};
            s.groupList = [];
            s.propertyList = [];
            s.propertyGroupList = [];
            s.roleList = [];
            s.rightList = [];
            s.originalPropertyList = [];
            s.bmRoleList = [];
            s.companyGroupList = [];
            s.productControlsList = [];
            s.productControlsMap = {};
            s.productPresetRolesMap = {};
            s.notificationsMap = {};
        };

        return new ProductDataSyncManager();
    }

    angular
        .module("settings")
        .factory("productDataSyncManager", [
            "pubsub",
            "routeSecurity",
            "personaDetails",
            factory
        ]);
})(angular);

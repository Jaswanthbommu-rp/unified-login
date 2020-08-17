//  Properties Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProductPropertiesGridCtrl($scope, $filter, gridModel, gridTransformSvc, gridPaginationModel, pubsub, persona, productDataModel, userDetailsModel, security, syncMgr, propertiesSvc, switchConfig) {
        var vm = this,
            hasViewUserAccess,
            allProperties,
            propertiesGrid = gridModel(),
            propertiesGridTransform = gridTransformSvc(),
            propertiesGridPagination = gridPaginationModel(),
            genericDataErrorReason = "",
            userLoginName = "",
            activeProperties = [],
            inactiveProperties = [];

        vm.init = function () {
            vm.propertySelect = "property"; //property
            vm.productId = 0;
            vm.activeProperties = activeProperties;
            vm.inactiveProperties = inactiveProperties;
            vm.allProperties = false;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");

            // console.log('PROPERTY');
            vm.propertiesGrid = propertiesGrid;
            propertiesGridTransform.watch(propertiesGrid);

            vm.config = syncMgr.getProductGridConfig($scope.$parent.productId, "Properties"); //configModel.getGridConfig()[0];
            logc("vm.config", vm.config, $scope.$parent.productId);
            propertiesGrid.setConfig(vm.config);
            propertiesGridPagination.setGrid(propertiesGrid);
            $scope.propertiesGridPagination = propertiesGridPagination;
            propertiesGridPagination.setConfig({
                recordsPerPage: 25
            });


            var radioconfig = syncMgr.getProductRadioConfig($scope.$parent.productId, "Properties");

            if (radioconfig !== undefined) {
                vm.radioconfig = syncMgr.getProductRadioConfig($scope.$parent.productId, "Properties");
            }

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.productPropertySwitchWatch = $scope.$watch(vm.isSwitchConfigLoaded, vm.setSwitchConfig);
            vm.productPropertyWatch = $scope.$watch(vm.isActive, vm.loadData);

            pubsub.subscribe("ppanel.access-type-change", vm.accessTypeChanged);
            pubsub.subscribe("ppanel.property-radio", vm.updatePropertyRecords);
            vm.gridAllWatch = propertiesGrid.subscribe("selectAll", vm.selectAllProperties);
            vm.gridSelectionWatch = propertiesGrid.subscribe("selectChange", vm.updateMultiSelectPropertyRecords);
            vm.filterData = propertiesGrid.subscribe("filterBy", vm.filter.bind(vm));
            vm.updateGridWatch = pubsub.subscribe("pplpropertygroup.updateGrids", vm.updateGrid);
            vm.accountingAllPropertiesSetWatch = pubsub.subscribe("acct.accountingAllPropertiesSet",vm.accountingAllPropertiesSet);
            vm.residentPortalAllPropertiesSetWatch = pubsub.subscribe("rp.residentPortalAllPropertiesSet",vm.setPropertySelect);
        };

        vm.accountingAllPropertiesSet = function(bool){
            vm.propertySelect = "";
            if(bool){
                vm.propertySelect = 'allProperties';
                syncMgr.setAccessTypeValue($scope.productId,vm.propertySelect);
            }
        };

        vm.productSelected = function (obj) {
            vm.productId = obj.productId;
            $scope.productId = obj.productId;
        };

        vm.accessTypeChanged = function (value) {
            vm.propertySelect = value;
            if (vm.propertySelect === 'allProperties') {
                vm.allProperties = true;
                syncMgr.allPropertiesSync($scope.productId, false);
            }
            else if (vm.propertySelect === 'property') {
                vm.allProperties = false;
            }
            else if (vm.propertySelect === 'propertyGroup') {
                syncMgr.allPropertiesSync($scope.productId, false);
            }
        };

        vm.hasViewOnlyAccess = function () {
            return security.isAllowed("viewUser") || syncMgr.isUserHasManageProductAccess($scope.$parent.productId);
        };

        vm.filter = function (filterBy) {
            if (vm.propertySelect === 'active') {
                vm.filteredRecords = $filter("filter")(vm.activeProperties, filterBy);
            }
            else if (vm.propertySelect === 'inactive') {
                vm.filteredRecords = $filter("filter")(vm.inactiveProperties, filterBy);
            }
            else {
                vm.filteredRecords = $filter("filter")(vm.allPropertiesData, filterBy);
            }

            propertiesGridPagination.setData(vm.filteredRecords).goToPage({
                number: 0
            });
        };

        vm.isReady = function () {
            return productDataModel.isPropertyGridActive();
        };

        vm.isActive = function () {
            return productDataModel.isPropertyGridActive();
        };

        vm.isSwitchConfigLoaded = function () {
            return syncMgr.isSwitchConfigLoaded();
        };

        vm.setPropertySelect = function(val){
            vm.propertySelect = val;
        };

        vm.hidePropertiesGrid = function () {
            if($scope.$parent.productId === 16 ||(vm.propertySelect === undefined && $scope.$parent.productId === 8)){
                var accesstype = syncMgr.getAccessTypeValue($scope.$parent.productId);
                if(accesstype){
                    vm.propertySelect = accesstype;
                }
            }
            if($scope.$parent.productId === 17 || $scope.$parent.productId === 23){
                var flag = syncMgr.isProductAllProperties($scope.$parent.productId);
                return flag;
            }
            if (vm.propertySelect === 'allProperties' && $scope.$parent.productId !== 9) {
                return true;
            }
            return false;
        };

        vm.isFinancialSuite = function () {
            return $scope.$parent.productId == 8;
        };

        vm.setSwitchConfig = function () {
            var productId = $scope.$parent.productId;
            vm.switchconfigs = syncMgr.getProductSwitchConfig(productId, "Properties");

            if (vm.switchconfigs !== undefined && vm.switchconfigs.length > 0) {
                vm.switchconfigs.forEach(function (item) {
                    item.configData = switchConfig({
                        onChange: vm.selectionAll,
                        disabled: vm.hasViewOnlyAccess()
                        //productId == 9 ? vm.updateNewPropertyByDefault :
                    });
                });

            }
        };

        vm.loadData = function () {
            var productId = $scope.$parent.productId;
            propertiesGrid.busy(true);
            if (persona.isReady() && vm.isActive()) {
                var propertyData = syncMgr.getProductPropertiesData(productId);

                if (propertyData === undefined) {
                    // propertiesGrid.busy(false);
                    var params = {
                        userPersonaId: userDetailsModel.getPersonaId(),
                        editorPersonaId: persona.getId(),
                        productId: productId,
                        userLoginName: userDetailsModel.getLoginName() === undefined ? userLoginName : userDetailsModel.getLoginName()
                    };

                    vm.dataPropReq = propertiesSvc.get(params, vm.setPropertyData);
                }
                else {
                    vm.loadGridData(productId);
                }
            }
        };

        vm.configEntityTypeFilters = function (propertyList){
            var distinctPropertyTypes = [];
            var propertytype = "";
            propertyList.forEach(function (property) {
                if(property.propertyType !== undefined){
                    propertytype = property.propertyType.replace(/ /g, "");
                    if(distinctPropertyTypes.indexOf(propertytype) == -1){
                        distinctPropertyTypes.push(propertytype);
                    }
                }
            });
            pubsub.publish("ppanel.distinct-entity-types", distinctPropertyTypes);
        };

        vm.setPropertyData = function (resp) {
            propertiesGrid.busy(false);
            if (resp.records && resp.records.length > 0) {
                var accesstype = syncMgr.getAccessTypeValue($scope.$parent.productId);
                if(accesstype === "allProperties" && $scope.$parent.productId === 16){
                    syncMgr.allPropertiesSync($scope.$parent.productId, false);
                    syncMgr.updateProductAllProperties($scope.$parent.productId, true);
                    resp.records.forEach(function (item) {
                        item.isAssigned = false;
                    });
                }
                var pdata = syncMgr.setPropertyList(resp.records, $scope.$parent.productId);
                if($scope.$parent.productId === 44){
                    vm.configEntityTypeFilters(resp.records[0].propertiesList);
                }
                if (resp.additional && resp.additional.allProperties) {
                    syncMgr.updateProductAllProperties($scope.$parent.productId, true);
                    vm.allProperties = true;
                }

                if (resp.additional && resp.additional.isAssignedNewPropertyByDefault) {
                    vm.allProperties = true;
                    syncMgr.updateProductNewPropertyByDefault($scope.$parent.productId, true);
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

        vm.loadGridData = function (productId) {
            propertiesGrid.busy(false);
            var propertySelect = "property";
            var activeCount = 0;
            var inActiveCount = 0;
            var propData = syncMgr.getProductPropertiesData(productId);
            var accesstype = syncMgr.getAccessTypeValue($scope.$parent.productId);

            if (propData && propData.length > 0) {
                propData.forEach(function (item) {
                    angular.extend(item, {
                        disableSelection: vm.hasViewOnlyAccess(),
                        radname: "property",
                        productId: productId,
                        originalProperty: item.isAssigned
                    });

                    
                    if(accesstype == "allProperties" && productId === 16){
                        item.isAssigned = false;
                    }
                    if (item.active !== undefined && productId === 10) {
                        if (item.active == 'true') {
                            vm.activeProperties.push(item);
                            if (item.isAssigned) {
                                activeCount++;
                            }
                        }
                        else {
                            vm.inactiveProperties.push(item);
                            if (item.isAssigned) {
                                inActiveCount++;
                            }
                        }
                    }
                    if (productId == "44" && item.propertiesList) {
                        var assignedPropertiesCount = item.propertiesList.filter(function (prop) {
                            return prop.isAssigned === true;
                        });
                        item.assignedProperties = assignedPropertiesCount.length + " of " + item.propertiesList.length;
                    }
                });

                if (syncMgr.isProductAllProperties(productId)) {
                    propertySelect = "allProperties";
                    vm.allProperties = true;

                    if(productId == 17){
                        //emit an event to enable switch in roles tab
                        pubsub.publish("rp.updateAllPropertiesSwitchSet", vm.allProperties);
                    }
                }

                if (syncMgr.isProductNewPropertyByDefault(productId)) {
                    propertySelect = "allProperties";
                    vm.allProperties = true;
                }

                vm.propertySelect = propertySelect;
                vm.allPropertiesData = propData;

                if (productId == "10") {
                    if (vm.propertySelect !== "allProperties") {
                        vm.propertySelect = inActiveCount > 0 ? "inactive" : "active";
                    }

                    if (vm.propertySelect == "active") {
                        propertiesGridPagination.setData(vm.activeProperties).goToPage({
                            number: 0
                        });
                        vm.propertiesGrid.filtersModel.reset();
                    }
                    else if (vm.propertySelect == "inactive") {
                        propertiesGridPagination.setData(vm.inactiveProperties).goToPage({
                            number: 0
                        });
                        vm.propertiesGrid.filtersModel.reset();
                    }
                }
                else {
                    propertiesGridPagination.setData(propData).goToPage({
                        number: 0
                    });
                }
            }

            return vm;
        };


        vm.showNotification = function () {
            return productDataModel.isPropertyGridActive() && $scope.$parent.productId === 3;
        };

        vm.selectionAll = function (bool) {
            vm.propertySelect = "property";
            if (bool) {
                vm.propertySelect = 'allProperties';
            }

            if ($scope.$parent.productId == 9) {
                syncMgr.updateProductNewPropertyByDefault($scope.$parent.productId, bool);
            }
            else {
                syncMgr.allPropertiesSync($scope.$parent.productId, bool);
            }
            vm.propertiesGrid.updateSelected();
            vm.resetProperties();
        };

        vm.selectAllProperties = function (val) {
            if (vm.filteredRecords !== undefined) {
                vm.filteredRecords.forEach(function (item) {
                    item.isAssigned = val;
                });

                syncMgr.updateAllProperties($scope.$parent.productId, vm.filteredRecords);
            }
            else {
                vm.allPropertiesData.forEach(function (item) {
                    item.isAssigned = val;
                });

                syncMgr.updateAllProperties($scope.$parent.productId, vm.allPropertiesData);
            }

            if($scope.$parent.productId == 8 && val){
                syncMgr.setAllPropertyGroupSync($scope.$parent.productId, val);
                pubsub.publish("acct.updateGridWatchSet");
            }
        };

        vm.updatePropertyRecords = function (record) {
            if (record) {
                var propertiesData = syncMgr.selectedPropertySync(record.productId, record);
            }
        };

        vm.updateNewPropertyByDefault = function (bool) {
            var propertiesData = syncMgr.updateProductNewPropertyByDefault($scope.$parent.productId, bool);
        };

        vm.updateMultiSelectPropertyRecords = function (record) {
            if (record) {
                var propertiesData = syncMgr.multiSelectedPropertySync(record.productId, record);
            }
        };

        vm.updateGrid = function () {
            vm.propertiesGrid.updateSelected();
        };

        vm.resetDataModel = function () {
            //vm.clearProperties();
            vm.resetProperties();
        };

        vm.resetProperties = function () {
            vm.allProperties = false;
            if (vm.propertySelect === 'allProperties') {
                vm.allProperties = true;
            }

            syncMgr.updateProductAllProperties($scope.$parent.productId, vm.allProperties);

            if (vm.propertySelect == "active") {
                propertiesGridPagination.setData(vm.activeProperties).goToPage({
                    number: 0
                });
                vm.propertiesGrid.filtersModel.reset();
            }
            else if (vm.propertySelect == "inactive") {
                propertiesGridPagination.setData(vm.inactiveProperties).goToPage({
                    number: 0
                });
                vm.propertiesGrid.filtersModel.reset();
            }
        };


        vm.destroy = function () {
            vm.destWatch();
            vm.personaWatch();
            vm.productPropertyWatch();
            vm.gridAllWatch();
            vm.gridSelectionWatch();
            vm.productPropertySwitchWatch();
            vm.updateGridWatch();
            //  vm.productSelectedWatch();
            if (vm.dataPropReq) {
                vm.dataPropReq.$cancelRequest();
            }

            propertiesGrid.destroy();
            propertiesGridTransform.destroy();
            propertiesGridPagination.destroy();
            propertiesGrid = undefined;
            propertiesGridTransform = undefined;
            propertiesGridPagination = undefined;
            vm.filteredPropertiesArray = [];
            //vm = undefined;
            //$scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProductPropertiesGridCtrl", [
            "$scope",
            "$filter",
            "rpGridModel",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "productPanelDataModel",
            "userDetailsModel",
            "routeSecurity",
            "productDataSyncManager",
            "productPropertiesSvc",
            "rpSwitchConfig",
            ProductPropertiesGridCtrl
        ]);
})(angular);

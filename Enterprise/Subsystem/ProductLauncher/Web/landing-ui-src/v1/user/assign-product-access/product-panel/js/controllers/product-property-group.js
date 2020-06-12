//  Property Groups Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProductPropertyGroupsGridCtrl($scope, $filter, dataSvc, pubsub, gridModel, gridTransformSvc, gridPaginationModel, security, persona, syncMgr, productDataModel, userDetailsModel, tabsModel) {
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
            vm.showPGGrid = true;
            vm.hideTab = false;

            pgGridPagination.setConfig({
                recordsPerPage: 25
            });

            pubsub.subscribe("ppanel.clearPropertyGroup", vm.clearPropertyGroup);
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.gridSelectionWatch = pgGrid.subscribe("selectChange", vm.selectionChange);
            vm.gridSelectAllWatch = pgGrid.subscribe("selectAll", vm.selectAllPropertyGroup);
            vm.filterData = pgGrid.subscribe("filterBy", vm.filter.bind(vm));
            vm.accountingAllPropertiesSetWatch = pubsub.subscribe("acct.accountingAllCompaniesSet",vm.accountingAllCompaniesSet);
            vm.updateGridWatch = pubsub.subscribe("acct.updateGridWatchSet",vm.updateGrid);
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

        vm.clearPropertyGroup = function(productId) {
            syncMgr.allPropertiesSync(productId, false);
            vm.updateGrid();
        };

        vm.accountingAllCompaniesSet = function(bool){
            vm.showPGGrid = !bool;
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
            if(productId == 8){
                var additionalData = syncMgr.getProductAdditionalData(productId);
                pubsub.publish("acct.accountingAdditionalDataSet",additionalData);
                if(additionalData["isMConsolePMC"] == false){
                    //hide companies tab and show entities tab for financial suite
                    vm.hideTab = true;
                    var allTabs = syncMgr.getProductAllTabs($scope.$parent.productId);
                    var initialTab = [];
                    var filteredAllTabs = allTabs.filter(function(tb){
                        if(tb.text != "Companies"){
                            if(tb.text == "Entities"){
                                tb.isActive = true;
                                initialTab.push(tb);
                            }
                            return tb;
                        }
                    });
                    
                    syncMgr.renderProductTabsMap($scope.$parent.productId, filteredAllTabs, initialTab);
                    syncMgr.renderProductActiveTabMap($scope.$parent.productId, initialTab);
                    vm.setProductTabs(filteredAllTabs);
                }
                if(additionalData["hasAccessToAllCurrentFutureProperties"]){
                    pubsub.publish("acct.accountingAllPropertiesSet",false);
                    vm.accountingAllCompaniesSet(false);
                }
            }
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
                if($scope.$parent.productId == 8){
                    syncMgr.setProductAdditionalData($scope.$parent.productId, resp.additional);
                }
                vm.loadGridData($scope.$parent.productId);
            }

            if (resp.isError) {
                vm.isPropertyGroupsError = true;
            }
            if(resp.additional) {
                var accesstype = resp.additional.accessType;
                pubsub.publish("ppanel.assign-accessType", accesstype);
            }
        };

        vm.setProductTabs = function (tabs) {
            var activeTab = syncMgr.getProductActiveTab($scope.$parent.productId);
            tabsModel.setTabs(tabs);
            tabsModel.setTabMenuData(tabs);
            tabsModel.activateTab(activeTab).initActiveTab();
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.activeWatch();
            vm.gridSelectionWatch();
            vm.gridSelectAllWatch();
            vm.hideTab = false;
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
            "pubsub",
            "rpGridModel",
            "rpGridTransform",
            "rpGridPaginationModel",
            "routeSecurity",
            "personaDetails",
            "productDataSyncManager",
            "productPanelDataModel",
            "userDetailsModel",
            "productPanelTabsModel",
            ProductPropertyGroupsGridCtrl
        ]);
})(angular);

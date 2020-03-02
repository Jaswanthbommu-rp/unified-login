//  User Controller

(function (angular, undefined) {
    "use strict";

    function ProductCommonCtrl($scope, $location, $params, view, session, pubsub, security, persona, productModel, jsonData ,panelModel, configData, configFactory, configModel, dataModel1, tabsModel, propertiesSvc, rolesSvc, userDetailsModel, adminJson) {
        var vm = this,
            active = false,
            panelNmae = "",
            productControls = "",
            tabsCnfData = [], gridconfigs = [], radioconfigs = [];

        vm.init = function () {
            vm.view = view;
            vm.security = security;
            vm.disableContent = false;
            vm.activeTab = "";
           // vm.productId = $params.productId;
            vm.productId = "";
            vm.tabsList = [];
            vm.tabsMenu = tabsModel.getTabsMenu();

            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            //vm.profileWatch = pubsub.subscribe("up.user-details-disable", vm.setState);
            vm.productSelectedWatch = pubsub.subscribe("product.selectedProduct", vm.productSelected );
            // if (persona.isReady()) {
            //     vm.loadProductControlsData(vm.productId);
            // }
            // else {
                 //vm.personaWatch = persona.subscribe(vm.loadProductControlsData(vm.productId));
            // }

            //vm.updateGridWatch = pubsub.subscribe("PA.updateGrids", vm.updateGrid);
        };

        vm.productSelected = function (obj) {
           // vm.personaWatch = persona.subscribe();
           logc("obj.productId",obj.productId);
           vm.productId = obj.productId;
           vm.loadProductControlsData(obj.productId);
           return vm;
        };

        vm.loadProductControlsData = function (productId)
        {
           // logc("tabsModel",tabsModel);
             tabsModel.reset();
            //productModel.setProductDataSelectKey(productId);
             //Check data in model for product
            var cdata = productModel.getProductControls(productId);
           // logc("cdata",cdata);
             var jData = "";
                if (productId == 10)
                {
                    jData = dataModel1;
                }
                else if (productId == 14){
                    jData = jsonData;
                }
                else if (productId == 3){
                    jData = adminJson;
                }
            if (cdata === undefined)
            {

                var s = productModel.setProductControlsList(jData);
                cdata = productModel.getProductControls(productId);
                //logc("cdata",cdata,cdata[0].DisplayName);
            }
            //logc("cdata",cdata);
            vm.panelName = productModel.getPageDisplayName(productId);// cdata[0].DisplayName;
           // logc("cdata panel name",vm.panelName);
            vm.setTabs(cdata.Controls[0]);
            //if no data in model,CALLSERVICE to get controls data
        };

        vm.isActive = function () {
            return true;// panelModel.isActive();
        };

        vm.setChanged = function () {
            panelModel.setChanged();
        };
        // Actions
        vm.setTabs = function (data) {

            vm.tabsCnfData = vm.getTabsConfigData(data);
            vm.gridconfigs = vm.getGridConfigs(vm.tabsCnfData);
            configModel.setGridConfig(vm.gridconfigs);
           // logc("gridconfigs",vm.gridconfigs);
           // logc("configModel",configModel);
            vm.radioconfigs = configData.getRadioConfig(data);
            //logc("cnfg for radio" ,vm.radioconfigs);

            if (vm.radioconfigs !== undefined){
                configModel.setRadioConfig(radioconfigs);
            }

            var tabData = vm.getProductTabsData(data);
            var tabs = tabsModel.setTabs(tabData);

            vm.tabsList = tabs.tabsList; //tabsDataSvc.getList();

            tabsModel.setTabMenuData(tabs.tabsList);
           // vm.tabsMenu = tabsModel.getTabsMenu();
            //logc("vm.tabsList",tabsModel, vm.tabsList[0].id);

            tabsModel.activateTab(vm.activeTab);
            //then set grids
            vm.getTabsGridData();
        };

        vm.getTabsGridData = function () {
            if (vm.tabsList){
                vm.tabsList.forEach(function (tab){
                    if (tab.id === "roles") {
                        vm.getProductRolesData();
                    }
                    if (tab.id === "properties") {
                        vm.getProductPropertiesData();
                    }
                });
            }
        };

        vm.getProductPropertiesData = function () {
              var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId(),
                    productId: vm.productId
                };

                vm.dataReq = propertiesSvc.get(params, vm.setPropertyData);
        };

        vm.getProductRolesData = function () {
              var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId(),
                    partyId: persona.data.organization.partyId,
                    productId: vm.productId
                };

                vm.dataReq = rolesSvc.get(params, vm.setRoleData);
        };

        vm.setPropertyData = function (resp) {
            if (resp.records && resp.records.length > 0){
               // logc("setPropertyData",resp.records, vm.productId);
                productModel.setPropertyList(resp.records, vm.productId);
                pubsub.publish("product.ProductPropertyData", vm.productId);
               // logc(productModel);
             }
        };

        vm.setRoleData = function (resp) {
            if (resp.records && resp.records.length > 0){
                productModel.setRoleList(resp.records, vm.productId);
                pubsub.publish("product.ProductRoleData", vm.productId);
               // logc(productModel);
             }
        };

        vm.getProductTabsData = function (data) {
            var  tabs = [], i = 0;
            if(data && data.Controls){
                if(data.Type === 'TabGroup'){
                    data.Controls.forEach(function(tabGrp){
                       // logc("tabgroup", tabGrp, tabGrp.ActiveTab);
                        if (tabGrp.ActiveTab) {
                            vm.activeTab = tabGrp.DisplayName.toLowerCase();
                        }
                       var tab = {
                            id : tabGrp.DisplayName.toLowerCase(),
                            text : tabGrp.DisplayName,
                            isActive : tabGrp.ActiveTab,
                            incUrl: "user/assign-product-access/product-panel/templates/" + tabGrp.DisplayName.toLowerCase() + ".html"};

                      tabs.push(tab);
                    });
                }
            }
            return tabs;
        };

        vm.getTabsConfigData = function (data) {
            var cnfg = {}, tabs = [];
            //logc("tab data",data);
            if(data && data.Controls){
                if(data.Type === 'TabGroup'){
                    data.Controls.forEach(function(tabGrp){
                        //logc("tabgroup data",tabGrp);
                      tabGrp.Controls.forEach(function (tab) {
                            if (tab.Type === "MultiSelectGrid"){
                                cnfg = configData.getGridConfigTypes(tab);
                                tabs.push(cnfg);
                            }
                        });
                    });
                }
            }
           // logc("tabs getTabsConfigData ", tabs);
            return tabs;
        };

        vm.getGridConfigs = function (tabsCfData) {
            var cnfgs = [];
            if(tabsCfData){
                tabsCfData.forEach(function (tab) {

                    var hdrCnfgs = {} , fltrCnfg = {}, mainCnfg = {} ;

                    var h = configData.getHeaders(tab);
                    hdrCnfgs = h;

                    var f = configData.getFilters(tab);
                    fltrCnfg = f;

                    var m = configData.getMain(tab);
                    mainCnfg = m;

                    var cnfg = {
                        "headers" : hdrCnfgs,
                        "filters" : fltrCnfg,
                        "main"    : mainCnfg
                    };

                    var c = configFactory(cnfg);
                    cnfgs.push(c);

                });
            }

           // logc("cnfg for ", cnfgs);
            return cnfgs;
        };

        vm.setState = function (value) {
            vm.disableContent = value;
        };
        // Assertions

        vm.hasAccess = function () {
            var allowed = true,
                calledFrom = $params.link;

            if (persona.hasViewOnlySupportToolAccess()){
                allowed = false;
            }

            return allowed;
        };

        // vm.hasMultipleTabs = function () {
        //     return false;//userTabs.hasMultipleTabs();
        // };

        vm.destroy = function () {
            vm.destWatch();
            vm.profileWatch();
            vm.productSelectedWatch();
            tabsModel.reset();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProductCommonCtrl", [
            "$scope",
            "$location",
            "$stateParams",
            "userViewModel",
            // "userTabsModel",
            // "userTabsManager",
            "userSessionModel",
            "pubsub",
            "routeSecurity",
            "personaDetails",
            "productDataSyncManager",
            "DataModel",
            "productPanelDataModel",
            "configDataModel",
            "gridConfigFactory",
            "ConfigModel",
           // "productPanelTabsMenu",
           // "productPanelTabsData",
            "DataModelpcc",
            "productPanelTabsModel",
            "productPropertiesSvc",
            "productRolesSvc",
            "userDetailsModel",
            "DataModel1",
            ProductCommonCtrl
        ]);
})(angular);

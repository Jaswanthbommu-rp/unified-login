//  User Controller

(function (angular, undefined) {
    "use strict";

    function ProductCommonCtrl($scope, $location, $params, view, session, pubsub, security, persona, productModel, jsonData ,panelModel, configData, configFactory, configModel, dataModel1, tabsModel, propertiesSvc, rolesSvc, userDetailsModel, adminJson, gridModel, gridTransformSvc, gridPaginationModel, switchConfig, jsonDataMc) {
        var vm = this,
            active = false,
            panelNmae = "",
            productControls = "",
            tabsCnfData = [],
            gridconfigs = [],
            radioconfigs = [],
            switchconfigs = [];

        vm.init = function () {
            vm.view = view;
            vm.security = security;
            vm.disableContent = false;
            vm.activeTab = "";

           // vm.productId = $params.productId;
            vm.productId = 0;
            vm.tabsList = [];
            vm.tabsMenu = tabsModel.getTabsMenu();

            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            //vm.profileWatch = pubsub.subscribe("up.user-details-disable", vm.setState);
            vm.productSelectedWatch = pubsub.subscribe("product.selectedProduct", vm.productSelected );

            //vm.updateGridWatch = pubsub.subscribe("PA.updateGrids", vm.updateGrid);
        };

        vm.productSelected = function (obj) {
           // vm.personaWatch = persona.subscribe();
           if (obj.productId == 14 || obj.productId == 10 ||  obj.productId == 9 ||  obj.productId == 3){

               logc("obj.productId",obj.productId);
               vm.productId = obj.productId;
               $scope.productId = obj.productId;
               vm.loadProductControlsData(obj.productId);
           }
           active = obj.productId === 14 || obj.productId === 10 || obj.productId === 9 || obj.productId === 3 ? true : false;
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
                else if (productId == 9){
                    jData = jsonDataMc;
                }
            if (cdata === undefined)
            {

                var s = productModel.setProductControlsList(jData);
                cdata = productModel.getProductControls(productId);
                //logc("cdata",cdata,cdata[0].DisplayName);
            }
            logc("cdata",cdata, cdata.controls[0]);
            vm.panelName = productModel.getPageDisplayName(productId);// cdata[0].DisplayName;
           // logc("cdata panel name",vm.panelName);
            vm.setTabs(cdata);
            //if no data in model,CALLSERVICE to get controls data


        };

        vm.isActive = function () {
            return active;// panelModel.isActive();
        };

        vm.getActiveUrl = function() {
            return tabsModel.getActiveUrl();
        };

        vm.setChanged = function () {
            panelModel.setChanged();
        };
        // Actions
        vm.setTabs = function (data) {

            var tNames = [];
            var names = [];
            panelModel.gridReset();
            vm.tabsCnfData = vm.getTabsConfigData(data,tNames, names );

            console.log("tNames", tNames);

            // vm.getAllConfigData(data);

            // vm.tabsCnfData.forEach(function (tab) {
            //   logc("tabdatata", tab);
            //   if (tab.gridName === "Roles"){
            //      vm.getGridConfigs(tab.gridConfig);
            //   }

            // });

            vm.switchconfigs = vm.getSwitchConfigs(data);
            configModel.setSwitchConfig(vm.switchconfigs);
            console.log("vm.switchconfigs", vm.switchconfigs);

            vm.gridconfigs = vm.getGridConfigs(vm.tabsCnfData);
            configModel.setGridConfig(vm.gridconfigs);

            vm.radioconfigs = configData.getRadioConfig(data);
//logc("vm.radioconfigs",vm.radioconfigs);
            if (vm.radioconfigs !== undefined){
                configModel.setRadioConfig(vm.radioconfigs);
            }

            var tabData = vm.getProductTabsData(data);
            var tabs = tabsModel.setTabs(tabData);

            vm.tabsList = tabs.tabsList;

            tabsModel.setTabMenuData(tabs.tabsList);

            tabsModel.activateTab(vm.activeTab);
            tabsModel.initActiveTab();
            //then set grids
            //vm.getTabsGridData();
            panelModel.setPropertyGridActive(true);
            panelModel.setRoleGridActive(true);

            var i = 0;

            tNames.forEach(function (t) {
                var j=0,k=0,l=0;
                names.forEach(function (name) {
                   if(t[name]){
                         vm.gridconfigs.forEach(function (g) {
                            if(i == j){
                                t[name].gridConfig = g;
                                // return;
                            }
                            j++;
                        });

                        vm.radioconfigs.forEach(function (r) {
                            if(i == k){
                                t[name].radioConfig = r;
                                // return;
                            }
                            k++;
                        });

                        vm.switchconfigs.forEach(function (s) {
                            if(i == l){
                                t[name].switchConfig = s;
                                // return;
                            }
                            l++;
                        });

                   }
                });
               
                i++;
            });

            configModel.setConfig(tNames);
            console.log("tNames with config", tNames);
        };

        // vm.getAllConfigData = function (data) {
            
        // };

        vm.getTabsGridData = function () {
            if (vm.tabsList){
                vm.tabsList.forEach(function (tab){
                    if (tab.id === "roles") {
                        vm.getProductRolesData();
                    }
                    // if (tab.id === "properties") {
                    //     vm.getPropertiesData();
                    // }
                });
            }
        };

        // vm.getPropertiesData = function () {
        //       var propertyData = productModel.getProductPropertiesData(vm.productId);
        //       logc("propertyData",propertyData,vm.productId);
        //       if (propertyData === undefined){
        //         var params = {
        //             userPersonaId: userDetailsModel.getPersonaId(),
        //             editorPersonaId: persona.getId(),
        //             productId: vm.productId
        //         };

        //         vm.dataPropReq = propertiesSvc.get(params, vm.setPropertyData);
        //       }
        //       panelModel.setPropertyGridActive(true);
        //       // else{
        //       //   //pubsub.publish("product.ProductPropertyData", vm.productId);

        //       // }
        // };

        vm.getProductRolesData = function () {
            var roleData = productModel.getProductRolesData(vm.productId);
            logc("roleData",roleData,vm.productId);
            if (roleData === undefined){
                 var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId(),
                    partyId: persona.data.organization.partyId,
                    productId: vm.productId
                };

                vm.dataRoleReq = rolesSvc.get(params, vm.setRoleData);
            }
            // pubsub.publish("product.ProductRoleData", vm.productId);
           panelModel.setRoleGridActive(true);

        };

        // vm.setPropertyData = function (resp) {
        //     if (resp.records && resp.records.length > 0){
        //        // logc("setPropertyData",resp.records, vm.productId);
        //         var pdata = productModel.setPropertyList(resp.records, vm.productId);
        //         panelModel.setPropertyGridActive(true);
        //         //pubsub.publish("product.ProductPropertyData", vm.productId);
        //        // logc(productModel);
        //      }
        // };

        vm.setRoleData = function (resp) {
            if (resp.records && resp.records.length > 0){
                var rdata = productModel.setRoleList(resp.records, vm.productId);
                panelModel.setRoleGridActive(true);
                //pubsub.publish("product.ProductRoleData", vm.productId);
               // logc(productModel);
             }
        };

        vm.getProductTabsData = function (data) {
            var  tabs = [], i = 0;
            if(data && data.controls){
                data.controls.forEach(function(tabControl) {
                    if(tabControl.type === 'Tab Group'){
                        tabControl.controls.forEach(function(tabGrp){
                           var activeTab = false;
                           if ( tabGrp.attributes !== null){
                                 tabGrp.attributes.forEach(function (item)  {
                                    logc("attributes", item);
                                   if (item.key === "Default" && item.value === "True"){
                                        vm.activeTab = tabGrp.displayName.toLowerCase();
                                        activeTab = true;
                                   }
                                 });
                           }
                           var tab = {
                                id : tabGrp.displayName.toLowerCase(),
                                text : tabGrp.displayName,
                                isActive : activeTab,
                                incUrl: "user/assign-product-access/product-panel/templates/" + tabGrp.displayName.toLowerCase() + ".html"};

                          tabs.push(tab);
                        });
                    }
                });
            }
            return tabs;
        };

        vm.getTabsConfigData = function (data, tNames, names) {
            var cnfg = {}, tabs = [];

            if(data && data.controls){
                data.controls.forEach(function (tabControl) {
                  if(tabControl.type === 'Tab Group'){
                        tabControl.controls.forEach(function(tabGrp){
                          var tabName = tabGrp.displayName;
                          names.push(tabName);
                          var a = [];
                          var o ={                          
                                gridConfig : [],
                                radioConfig: [],
                                switchConfig: []                            
                          };
                          a[tabName] = o;
                          tNames.push(a);

                          tabGrp.controls.forEach(function (tab) {
                                if (tab.type === "MultiSelectGrid" || tab.type === "Select Grid"){
                                    cnfg = configData.getGridConfigTypes(tab, tabName);
                                    tabs.push(cnfg);
                                }
                            });
                        });
                    }
                });
            }
            return tabs;
        };

        vm.getGridConfigs = function (tabsCfData) {
            var cnfgs = [];
            logc("tabsCfData",tabsCfData);
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

        vm.getSwitchConfigs = function (data) {
            var  aSwitch = []; 
            if(data && data.controls){
                if(data.type === 'Tab Group'){
                    data.controls.forEach(function(tabGrp){
                        var s = []; 
                       tabGrp.controls.forEach(function (ctrl) {
                           if(ctrl.type === 'Switch'){
                               var c = {
                                    id : ctrl.id,
                                    text : ctrl.displayName,
                                    key : ctrl.dataSource,
                                    configData : switchConfig({
                                       onChange : vm.noop,
                                       disabled : false
                                    })
                                };
                                s.push(c);
                            }
                       });
                       aSwitch.push(s);

                    });
                }
            }
            return aSwitch;
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
            logc("destroy called");
            if (vm.dataPropReq) {
                vm.dataPropReq.$cancelRequest();
            }

            if (vm.dataRoleReq) {
                vm.dataRoleReq.$cancelRequest();
            }

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
            "rpSwitchConfig",
            "DataModelMc",
            ProductCommonCtrl
        ]);
})(angular);

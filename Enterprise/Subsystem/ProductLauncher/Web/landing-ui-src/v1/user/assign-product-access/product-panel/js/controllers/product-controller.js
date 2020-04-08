//  User Controller

(function (angular, undefined) {
    "use strict";

    function ProductCommonCtrl($scope, $location, $params, view, session, pubsub, security, persona, productModel, panelModel, configData, configFactory, tabsModel, userDetailsModel, switchConfig, cntrlSvc, templateModel) {
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

            vm.productId = 0;
            vm.tabsList = [];
            vm.tabsMenu = tabsModel.getTabsMenu();

            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.productSelectedWatch = pubsub.subscribe("product.selectedProduct", vm.productSelected);
        };

        vm.productSelected = function (obj) {
            var productExists = templateModel.isProductExists(obj.productId);
            if (productExists) {
                vm.productId = obj.productId;
                $scope.productId = obj.productId;
                vm.loadProductControlsData(obj.productId);
            }
            logc("productExists", productExists, obj.productId);
            active = productExists ? true : false;
            return vm;
        };

        vm.loadProductControlsData = function (productId) {
            tabsModel.reset();
            //Check data in model for product
            var controlData = productModel.getProductControls(productId);

            if (controlData === undefined) {

                var params = {
                    productId: productId
                };

                vm.dataCntrlsReq = cntrlSvc.get(params, vm.setControlsData);
            }
            //logc("cdata", cdata, cdata.controls[0]);
            if (controlData !== undefined) {
                vm.panelName = productModel.getPageDisplayName(productId);
                vm.setTabs(controlData);
            }
        };

        vm.isActive = function () {
            return active; // panelModel.isActive();
        };

        vm.getActiveUrl = function () {
            return tabsModel.getActiveUrl();
        };

        vm.setChanged = function () {
            panelModel.setChanged();
        };
        // Actions
        vm.setControlsData = function (resp) {
            if (resp && resp.controls) {
                productModel.setProductControlsList(resp);
                var controlData = productModel.getProductControls(vm.productId);
                vm.panelName = productModel.getPageDisplayName(vm.productId);
                vm.setTabs(controlData);
            }
        };

        vm.setTabs = function (data) {

            panelModel.gridReset();

            vm.setTabsConfigData(data);
            vm.setSwitchConfigs(data);

            var tabData = vm.getProductTabsData(data);
            var tabs = tabsModel.setTabs(tabData);

            vm.tabsList = tabs.tabsList;
logc("vm.tabsList", vm.tabsList);
            tabsModel.setTabMenuData(tabs.tabsList);
            tabsModel.activateTab(vm.activeTab).initActiveTab();

            panelModel.setPropertyGridActive(true);
            panelModel.setRoleGridActive(true);
        };

        vm.getProductTabsData = function (data) {
            var allTabs = [],
                initialTabs = [],
                i = 0;
            if (data && data.controls) {
                data.controls.forEach(function (tabControl) {
                    if (tabControl.type === 'Tab Group') {
                        tabControl.controls.forEach(function (tabGrp) {
                            var activeTab = false;
                            var hideTab = false;
                            if (tabGrp.attributes !== null && tabGrp.type === "Tab") {
                                tabGrp.attributes.forEach(function (item) {
                                    if (item.key === "Default" && item.value === "True") {
                                        vm.activeTab = tabGrp.displayName.toLowerCase();
                                        activeTab = true;
                                    }
                                    if (item.key === "Hide" && item.value === "True") {
                                       hideTab = true;
                                    }
                                });
                            }
                            var tab = {
                                id: tabGrp.displayName.toLowerCase(),
                                text: tabGrp.displayName,
                                isActive: activeTab,
                                incUrl: "user/assign-product-access/product-panel/templates/" + tabGrp.displayName.replace(/ /g, "").toLowerCase() + ".html"
                            };
                            allTabs.push(tab);
                            if (!hideTab) {
                                initialTabs.push(tab);
                            }
                        });
                    }
                });
                productModel.renderProductTabsMap($scope.productId, allTabs, initialTabs);
                productModel.renderProductActiveTabMap($scope.productId, vm.activeTab);
            }
            return allTabs;
        };

        vm.setTabsConfigData = function (data) {
            var productId = $scope.productId;
            if (data && data.controls) {
                data.controls.forEach(function (tabControl) {
                    if (tabControl.type === 'Tab Group') {
                        tabControl.controls.forEach(function (tabGrp) {
                            var tabName = tabGrp.displayName.replace(/ /g, "");

                            tabGrp.controls.forEach(function (tab) {
                                if (tab.type === "Multi Select Grid" || tab.type === "Select Grid") {
                                    //Check and Set Grid Config Types
                                    var showSelectAll = false;
                                    if (tab.attributes !== null) {
                                        tab.attributes.forEach(function (item) {
                                            logc("attributes", item);
                                            if (item.key === "ShowSelectAll" && item.value === "True") {
                                                showSelectAll = true;
                                            }
                                        });
                                    }
                                    if (productModel.getProductGridConfig(productId, tabName) === undefined) {
                                        var cnfg = configData.getGridConfigTypes(tab, tabName);
                                        var gridConfig = vm.getGridConfig(cnfg, showSelectAll);
                                        productModel.renderProductGridConfigMap(productId, tabName, gridConfig);
                                        vm.setProductDependency(tab, productId);
                                    }

                                    //Check and Set any Aside List Grid
                                    if (productModel.getProductAsideGridConfig(productId, tabName) === undefined) {
                                        var listAsideconfigs = configData.getListAsideConfig(tab);

                                        if (listAsideconfigs !== undefined &&
                                            listAsideconfigs.config.length > 0) {
                                            var asideGridConfig = vm.getGridConfig(listAsideconfigs.config, showSelectAll);
                                        logc("asideGridConfig", asideGridConfig);
                                            productModel.renderProductAsideGridConfigMap(productId, tabName, asideGridConfig, listAsideconfigs.displayName);
                                        }
                                    }
                                }
                            });

                            //Check and Set any radio data
                            if (productModel.getProductRadioConfig(productId, tabName) === undefined) {
                                var radioCnfg = configData.getRadioConfig(tabGrp);
                                productModel.renderProductRadioConfigMap(productId, tabName, radioCnfg);
                            }
                        });
                    }
                });
            }
        };

        vm.setProductDependency = function (gridData, productId) {
            // logc("griddata--", gridData,gridData.Type);
            if (gridData.type === "Multi Select Grid" || gridData.type === "Select Grid") {
                gridData.controls.forEach(function (item) {
                    if (item.dependency !== null && item.dependency) {
                        productModel.renderProductDependencyMap(productId, item.displayName.toLowerCase(), item.id);
                    }
                });
            }
        };

        vm.getGridConfig = function (data, showSelectAll) {
            var cnfgs = [];

            if (data) {
                var hdrCnfgs = {},
                    fltrCnfg = {},
                    mainCnfg = {};

                var h = configData.getHeaders(data, showSelectAll);
                hdrCnfgs = h;

                var f = configData.getFilters(data);
                fltrCnfg = f;

                var m = configData.getMain(data);
                mainCnfg = m;

                var cnfg = {
                    "headers": hdrCnfgs,
                    "filters": fltrCnfg,
                    "main": mainCnfg
                };

                var c = configFactory(cnfg);
                cnfgs.push(c);
            }

            return cnfgs;
        };

        vm.setSwitchConfigs = function (data) {
            var aSwitch = [];
            //Check and Set any Switch
            if (data && data.controls) {
                data.controls.forEach(function (tabControl) {
                    if (tabControl.type === 'Tab Group') {
                        tabControl.controls.forEach(function (tabGrp) {
                            aSwitch = [];
                            var tabName = tabGrp.displayName.replace(/ /g, "");
                            if (productModel.getProductSwitchConfig($scope.productId, tabName) === undefined) {
                                tabGrp.controls.forEach(function (ctrl) {
                                    if (ctrl.type === 'Switch') {
                                        logc("switch control", ctrl);
                                        var c = {
                                            id: ctrl.id,
                                            text: ctrl.displayName,
                                            key: ctrl.dataSource,
                                            configData: switchConfig({
                                                onChange: vm.noop,
                                                disabled: false
                                            })
                                        };
                                        aSwitch.push(c);
                                    }
                                });
                                //logc("aSwitch", aSwitch);
                                if (aSwitch.length > 0) {
                                    logc("aSwitch", aSwitch);
                                    productModel.renderProductSwitchConfigMap($scope.productId, tabName, aSwitch);
                                }
                            }
                        });
                    }
                });
            }
            // return aSwitch;
        };

        vm.setState = function (value) {
            vm.disableContent = value;
        };

        // Assertions

        vm.hasAccess = function () {
            var allowed = true,
                calledFrom = $params.link;

            if (persona.hasViewOnlySupportToolAccess()) {
                allowed = false;
            }

            return allowed;
        };

        // vm.hasMultipleTabs = function () {
        //     return false;//userTabs.hasMultipleTabs();
        // };

        vm.destroy = function () {
            logc("vm.destroy called");
            active = false;
            vm.destWatch();
            vm.profileWatch();
            vm.productSelectedWatch();
            if (vm.dataCntrlsReq) {
                vm.dataCntrlsReq.$cancelRequest();
            }
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
            "userSessionModel",
            "pubsub",
            "routeSecurity",
            "personaDetails",
            "productDataSyncManager",
            "productPanelDataModel",
            "configDataModel",
            "gridConfigFactory",
            "productPanelTabsModel",
            "userDetailsModel",
            "rpSwitchConfig",
            "productControlsSvc",
            "productTemplateModel",
            ProductCommonCtrl
        ]);
})(angular);

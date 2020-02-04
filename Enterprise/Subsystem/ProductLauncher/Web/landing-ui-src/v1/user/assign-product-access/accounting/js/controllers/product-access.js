//  AccountingProductAccess Controller

(function (angular, undefined) {
    "use strict";

    function AccountingProductAccessCtlr($scope, $filter, 
        
        ADataModel, persona, switchConfig, security, switchModel, pubsub, tabsModel) {
        var vm = this;

        vm.init = function () {
            switchModel.refresh();
            
            vm.panelName = $filter("productPanelText")("panelName.accounting");
            
            var tabs = ["companies", "entities", "roles"];
            tabsModel.setTabs(tabs);
            tabsModel.activateTab("companies");
            vm.tabsList = tabsModel.getTabsList();
            vm.tabsMenu = tabsModel.getTabsMenu();
           
            vm.allProperties = false;
                        
            vm.acessSiteSpndMgmtOnly = false;
            vm.accountingAdmin = false;
            vm.siteSpendManagementAssignedToCompany = switchModel.siteSpendManagementAssignedToCompany;

            vm.allPropertiesSwitchWatch = pubsub.subscribe("Acct.allPropertiesSwitchWatch", vm.allPropertiesSwitchWatch);
            vm.acessSiteSpndMgmtOnlySwitchWatch = pubsub.subscribe("Acct.acessSiteSpndMgmtOnlySwitchWatch", vm.acessSiteSpndMgmtOnlySwitchWatch);
            vm.accountingAdminSwitchWatch = pubsub.subscribe("Acct.accountingAdminSwitchWatch", vm.accountingAdminSwitchWatch);
            vm.showHideTabsAdminSwitchWatch = pubsub.subscribe("Acct.showTabs", vm.showHideTabs);
            

            vm.personaWatch = angular.noop;

            if (persona.isReady()) {
                vm.loadToggles();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadToggles());
            }            
            
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.showHideTabs = function (tabs) {
            tabsModel.setTabs(tabs);
            tabsModel.activateTab("entities");
            vm.tabsList = tabsModel.getTabsList();
            vm.tabsMenu = tabsModel.getTabsMenu();
        };

        vm.loadToggles = function () {
            vm.allPropSwitch = switchConfig({
                onChange: vm.setAllProperties,
                disabled: security.isAllowed("viewUser") || !vm.isUserHasManageProductAccess()
            });

            vm.acessSiteSpndMgmtOnlySwitch = switchConfig({
                onChange: vm.setSiteSpndMgmt,
                disabled: security.isAllowed("viewUser") || !vm.isUserHasManageProductAccess()
            });

            vm.accountingAdminSwitch = switchConfig({
                onChange: vm.setAccountingAdmin,
                disabled: security.isAllowed("viewUser") || !vm.isUserHasManageProductAccess()
            });

        };

        vm.setCompEntRoles = function (comp, ent, role) {
             // Set companies tab view 
                pubsub.publish("Acct.showCompanies", comp);

                // Set entities tab view 
                pubsub.publish("Acct.showEntities", ent);

                // Set Roles tab view 
                pubsub.publish("Acct.showRoles", role);
        };

        vm.setAllProperties = function (val) {
             switchModel.setHasAccessToCurrentFutureProp(val);
             
             if(val){   
                vm.setCompEntRoles(false, false, true);             
             }else{                
                 vm.setCompEntRoles(true, true, true);
             }       
            
        };

         vm.setSiteSpndMgmt = function (val) {
            if (val) {                
                switchModel.setIsAccountingAdmin(false);
                vm.accountingAdmin = false;
            }     
            switchModel.setHasAccessToSiteSpendMgmtOnly(val);            
        };

         vm.setAccountingAdmin = function (val) {
             switchModel.setIsAccountingAdmin(val);
             
            if (val) {

                if(!vm.allProperties){                                        
                    vm.setCompEntRoles(true, true, true);
                }else{
                    // when both toggles                   
                    vm.setCompEntRoles(false, false, true);
                }

                switchModel.setHasAccessToSiteSpendMgmtOnly(false);
                vm.acessSiteSpndMgmtOnly = false;      
                
            }
            else {
                // Set entities tab view              

                if(!vm.allProperties){                   
                    vm.setCompEntRoles(true, true, true);
                }else{                    
                    vm.setCompEntRoles(false, false, true);
                }
                 
            }
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageAccountingProductAccessCtlr;
        };

        vm.GetSiteSpendManagementAssignedToCompany = function () {
            return switchModel.getIsSiteSpendManagementAssignedToCompany();
        };


        vm.accountingAdminSwitchWatch = function (val) {  
            vm.accountingAdmin = val;
        };

        vm.acessSiteSpndMgmtOnlySwitchWatch = function (val) {
           vm.acessSiteSpndMgmtOnly = val;
        };

        vm.allPropertiesSwitchWatch = function (val) {        
            vm.allProperties = val;
        };
       

        vm.isActive = function () {
            return ADataModel.isActive();
        };

        vm.setChanged = function () {
            ADataModel.setChanged();
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.personaWatch();
            vm.allPropertiesSwitchWatch();
            vm.acessSiteSpndMgmtOnlySwitchWatch();
            vm.accountingAdminSwitchWatch();
            vm.showHideTabsAdminSwitchWatch();            
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("AccountingProductAccessCtlr", [
            "$scope",
            "$filter",            
            "AccountingDataModel",
            "personaDetails",
            "rpSwitchConfig",
            "routeSecurity",
            "ASwitchModel",
            "pubsub",
            "AcctTabsModel",
            AccountingProductAccessCtlr
        ]);
})(angular);

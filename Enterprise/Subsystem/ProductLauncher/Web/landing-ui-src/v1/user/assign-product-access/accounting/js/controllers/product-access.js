//  AccountingProductAccess Controller

(function (angular, undefined) {
    "use strict";

    function AccountingProductAccessCtlr($scope, $filter, tabsMenuModel, navData, ADataModel, persona, switchConfig, security, switchModel, pubsub) {
        var vm = this;

        vm.init = function () {
            switchModel.refresh();
            vm.tabsList = [];
            vm.panelName = $filter("productPanelText")("panelName.accounting");
            vm.tabsMenu = tabsMenuModel().setData(navData.getList());
            vm.tabsList = navData.getList();
           
            vm.allProperties = false;
                        
            vm.acessSiteSpndMgmtOnly = false;
            vm.accountingAdmin = false;
            vm.siteSpendManagementAssignedToCompany = switchModel.siteSpendManagementAssignedToCompany;

            vm.allPropertiesSwitchWatch = pubsub.subscribe("Acct.allPropertiesSwitchWatch", vm.allPropertiesSwitchWatch);
            vm.acessSiteSpndMgmtOnlySwitchWatch = pubsub.subscribe("Acct.acessSiteSpndMgmtOnlySwitchWatch", vm.acessSiteSpndMgmtOnlySwitchWatch);
            vm.accountingAdminSwitchWatch = pubsub.subscribe("Acct.accountingAdminSwitchWatch", vm.accountingAdminSwitchWatch);
            
            vm.personaWatch = angular.noop;

            if (persona.isReady()) {
                vm.loadToggles();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadToggles());
            }            
            
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
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

        vm.setAllProperties = function (val) {
             switchModel.setHasAccessToCurrentFutureProp(val);
             ADataModel.clearProperties();
             if(val){

                if(!vm.accountingAdmin){
                    // Set companies tab view to true
                    pubsub.publish("Acct.showCompanies", false);

                    // Set entities tab view to false
                    pubsub.publish("Acct.showEntities", false);

                    // Set Roles tab view to true
                    pubsub.publish("Acct.showRoles", true);
                }else{
                            // when both are true
                        logc("vm.accountingAdmin", vm.accountingAdmin, "val", val);
                }
             
             }else{
                if(!vm.accountingAdmin){
                    // Set companies tab view to true
                    pubsub.publish("Acct.showCompanies", true);

                    // Set entities tab view to true
                    pubsub.publish("Acct.showEntities", true);

                    // Set Roles tab view to true
                    pubsub.publish("Acct.showRoles", true  );
                }else{
                     // Set companies tab view to true
                    pubsub.publish("Acct.showCompanies", true);

                    // Set entities tab view to false
                    pubsub.publish("Acct.showEntities", false);

                    // Set Roles tab view to true
                    pubsub.publish("Acct.showRoles", true);
                        logc("vm.accountingAdmin", vm.accountingAdmin, "val", val);
                }

             }

             pubsub.publish("Acct.allCompChange");  

            if (val) {
                //clear grid selections company/entities, if theres any
                pubsub.publish("Acct.allEntChange");
            }
            else {
                if(switchModel.getIsAccountingAdmin() === true){
                    // switchModel.setIsAccountingAdmin(false);
                    // vm.accountingAdmin = false;
                    ADataModel.clearRoles();
                }                
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
             // ADataModel.setAccountingAdmin(val);
             

            if (val) {
                
                // switchModel.setHasAccessToCurrentFutureProp(val);
                // vm.allProperties = true;        

                if(!vm.allProperties){
                    
                    // Set companies tab view to true
                    pubsub.publish("Acct.showCompanies", true);

                    // Set entities tab view to false
                    pubsub.publish("Acct.showEntities", false);

                    // Set Roles tab view to true
                    pubsub.publish("Acct.showRoles", true);
                }else{
                    // when both are true
                        logc("vm.allProperties", vm.allProperties, "val", val);
                }

                switchModel.setHasAccessToSiteSpendMgmtOnly(false);
                vm.acessSiteSpndMgmtOnly = false;        

                ADataModel.clearRoles();
                ADataModel.clearProperties();  

                //clear grid selections company/entities, if theres any
                pubsub.publish("Acct.allCompEntChange");
                //clear grid selections Roles, if theres any
                pubsub.publish("Acct.allRolesChange");
            }
            else {
                // Set entities tab view to false
                 

                 if(!vm.allProperties){
                    
                    // Set companies tab view to true
                    pubsub.publish("Acct.showCompanies", true);

                    // Set entities tab view to true
                    pubsub.publish("Acct.showEntities", true);

                    // Set Roles tab view to true
                    pubsub.publish("Acct.showRoles", true);
                }else{
                     // Set companies tab view to false
                    pubsub.publish("Acct.showCompanies", false);

                    // Set entities tab view to false
                    pubsub.publish("Acct.showEntities", false);

                    // Set Roles tab view to true
                    pubsub.publish("Acct.showRoles", true);

                        logc("vm.allProperties", vm.allProperties, "val", val);
                }

                 ADataModel.clearRoles();
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
        

        vm.getActiveUrl = function () {
            return navData.getActiveUrl();
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
            navData.reset();
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
            "rpScrollingTabsMenuModel",
            "ANavModel",
            "AccountingDataModel",
            "personaDetails",
            "rpSwitchConfig",
            "routeSecurity",
            "ASwitchModel",
            "pubsub",
            AccountingProductAccessCtlr
        ]);
})(angular);

//  Companies Grid Tab Controller

(function(angular, undefined) {
    "use strict";

    function ACompaniesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, ADataModel, switchConfig, userDetailsModel, security, switchModel, pubsub) {
        var vm = this,
            filteredRecords,
            companiesGrid = gridModel(),
            companiesGridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel();

        vm.init = function() {

            vm.allProperties = switchModel.allowAccessToCurrentFutureProp;
            vm.isShowCompanies = true;
            vm.companiesError = $filter("productPanelText")("panelError.generic");
            vm.companiesGrid = companiesGrid;
            companiesGridTransform.watch(companiesGrid);
            companiesGrid.setConfig(gridConfig);
            vm.gridPagination = gridPagination;
            gridPagination.setGrid(companiesGrid);
            $scope.gridPagination = gridPagination;
            gridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);
            vm.allCompWatch = pubsub.subscribe("Acct.allCompChange", vm.clearGridSelections);
            vm.showCompWatch = pubsub.subscribe("Acct.showCompanies", vm.showCompanies);
            
            vm.setAllCompaniesGrid = pubsub.subscribe("Acct.setAllCompaniesGridValue",vm.setGridSelections);
            vm.gridSelectionWatch = vm.companiesGrid.subscribe("selectChange", vm.gridRowSelectionChange);
            vm.propChangeWatch = pubsub.subscribe("Acct.propChange", vm.changeCompSelection);
            // vm.gridSelectAllWatch = vm.companiesGrid.subscribe("selectAll", vm.gridSelectAllChange);
            vm.filterData = vm.companiesGrid.subscribe("filterBy", vm.filter.bind(vm));

            if (persona.isReady()) {
                vm.loadData();
            } else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }
            vm.gridAllWatch = companiesGrid.subscribe("selectAll", vm.selectAllCompanies);
        };

        vm.showCompanies = function (val) {
            vm.isShowCompanies = val;
        };

        vm.gridRowSelectionChange = function(val) {            
            var comps = [];
            switchModel.getCompanies().forEach(function (comp) {
                comps.push(comp);
            });
            ADataModel.setCompanies(comps);
            // pubsub.publish("Acct.compChange", val);
        };

        vm.isAllProperties = function() {
            return switchModel.getAllProperties();
        };

        vm.clearGridSelections = function() {
            //clear selections, if theres any
            vm.companiesGrid.selectAll(false);
            vm.companiesGrid.updateSelected();
        };

        vm.setGridSelections = function(val) {
            vm.companiesGrid.selectAll(val);
            vm.companiesGrid.updateSelected();
        };

        vm.filter = function(filterBy){
            vm.filteredRecords = $filter("filter")(vm.dataReq.records, filterBy);
        };

        vm.isUserHasManageProductAccess = function() {
            return !persona.data.hasManageAccountingProductAccess;
        };


        vm.isActive = function() {
            return ADataModel.isActive();
        };

        vm.loadData = function() {
            if (persona.isReady() && ADataModel.isActive()) {
                companiesGrid.busy(true);
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId()
                };

                vm.activeWatch();
                vm.personaWatch();
                vm.dataReq = dataSvc.get(params, vm.setData);
            }
        };

        vm.setData = function(resp) {
            companiesGrid.busy(false);
            vm.setIsSiteSpendManagementAssignedToCompany(resp);
            vm.setOptionsFlags(resp);
            if (resp.records && resp.records.length > 0) {

                switchModel.setCompanies(resp.records);
                gridPagination.setData(resp.records).goToPage({
                    number: 0
                });

                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    vm.setViewUserState(resp);
                } else {
                    if (resp.additional ) {
                                                
                        ADataModel.setMConsole(resp.additional.isMConsolePMC) ;
                        // if isMConsolePMC is false , hide companies
                        if(!resp.additional.isMConsolePMC){
                             var activeTabs = ["entities","roles"];
                             pubsub.publish("Acct.showTabs", activeTabs);
                        }

                        if(resp.additional.isAccountingAdmin && !resp.additional.hasAccessToAllCurrentFutureProperties){
                             pubsub.publish("Acct.showEntities", true);
                        }

                        if(resp.additional.hasAccessToAllCurrentFutureProperties && !resp.additional.isAccountingAdmin){
                             pubsub.publish("Acct.showEntities", false);
                             pubsub.publish("Acct.showCompanies", false);
                        }

                        if(resp.additional.hasAccessToAllCurrentFutureProperties && resp.additional.isAccountingAdmin){
                             pubsub.publish("Acct.showEntities", false);
                             pubsub.publish("Acct.showCompanies", false);
                             pubsub.publish("Acct.showRoles", true);
                        }

                    } else {
                        vm.allProperties = false;
                        ADataModel.setCompanies(resp.records);
                    }
                }
            }
            if (resp.isError) {
                vm.iscompaniesError = true;
            }
        };

        vm.setOptionsFlags = function(resp) {
            if (resp.additional) {
                switchModel.setHasAccessToSiteSpendMgmtOnly(resp.additional.hasAccessToSiteSpendManagementOnly);
                switchModel.setHasAccessToCurrentFutureProp(resp.additional.hasAccessToAllCurrentFutureProperties);
                switchModel.setIsAccountingAdmin(resp.additional.isAccountingAdmin);

                pubsub.publish("Acct.allPropertiesSwitchWatch", resp.additional.hasAccessToAllCurrentFutureProperties);
                pubsub.publish("Acct.acessSiteSpndMgmtOnlySwitchWatch", resp.additional.hasAccessToSiteSpendManagementOnly);
                pubsub.publish("Acct.accountingAdminSwitchWatch", resp.additional.isAccountingAdmin);

            }
        };

        vm.setIsSiteSpendManagementAssignedToCompany = function(data) {
            if (data.additional) {
                switchModel.setIsSiteSpendManagementAssignedToCompany(data.additional.isSiteSpendManagementAssignedToCompany);
            }
        };

        vm.setAllCompanies = function(val) {
            if (val) {

                //clear selections, if theres any
                vm.companiesGrid.selectAll(false);
                vm.companiesGrid.updateSelected();
            } else {
                ADataModel.setCompanies(vm.dataReq.records);
            }
        };

        vm.selectAllCompanies = function (val) {
            //ADataModel.setallCompanies(vm.dataReq.records, val);
            if(vm.filteredRecords !== undefined){
                ADataModel.setallCompanies(vm.filteredRecords, val);
            }
            else{
                ADataModel.setallCompanies(vm.dataReq.records, val);
            } 
        };

        vm.setViewUserState = function(data) {
            data.records.forEach(function(item) {
                angular.extend(item, {
                    disableSelection: false
                });
                item.disableSelection = true;
            });
            vm.allProperties = false;
            ADataModel.setCompanies(data.records);
        };

        vm.gridSelectAllChange = function(val) {

            if (ADataModel.getCompanies() != undefined) {
                ADataModel.getCompanies().forEach(function(comp) {

                    if (ADataModel.getProperties() != undefined) {

                        var records = $filter("filter")(ADataModel.getProperties(), {
                            companyId: comp.id
                        }, true);

                        records.forEach(function(item) {
                            item.isAssigned = val;
                        });
                    }

                });
            }

        };


        vm.changeCompSelection = function(prop) {

            if (ADataModel.getCompanies() != undefined && prop != undefined) {

                var companies = $filter("filter")(ADataModel.getCompanies(), {
                    id: prop.companyId
                }, true);

               
                if (companies.length > 0 && prop.isAssigned === true) {
                    var propertiesByComp = $filter("filter")(ADataModel.getProperties(), {
                        companyId: prop.companyId
                    }, true);

                    var properties = $filter("filter")(propertiesByComp, {
                        isAssigned: true  // Any prop assigned
                    }, true);

                   
                    if (properties.length > 0) {
                        companies[0].isAssigned = true;
                    }
                }


                vm.goToPage();
            }
        };

        vm.goToPage =function () {
            vm.gridPagination.setGridData();
        };

        vm.destroy = function() {
            vm.destWatch();
            vm.allCompWatch();
            vm.gridAllWatch();
            vm.showCompWatch();
            // vm.gridSelectionWatch();
            // vm.gridSelectAllWatch();
            vm.propChangeWatch();

            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }
            companiesGrid.destroy();
            companiesGridTransform.destroy();
            gridPagination.destroy();
            companiesGrid = undefined;
            companiesGridTransform = undefined;
            gridPagination = undefined;
            vm = undefined;
            $scope = undefined;
            // vm.filteredRecords = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ACompaniesGridCtrl", [
            "$scope",
            "$filter",
            "ACompaniesSvc",
            "rpGridModel",
            "ACompaniesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "AccountingDataModel",
            "rpSwitchConfig",
            "userDetailsModel",
            "routeSecurity",
            "ASwitchModel",
            "pubsub",
            ACompaniesGridCtrl
        ]);
})(angular);

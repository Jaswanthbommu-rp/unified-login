//  Entities Grid Tab Controller

(function(angular, undefined) {
    "use strict";

    function AEntitiesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, ADataModel, switchConfig, userDetailsModel, security, switchModel, pubsub) {
        var vm = this,
            filteredRecords,
            entitiesGrid = gridModel(),
            entitiesGridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function() {
            vm.allProperties = true;
            vm.isShowEntities = true;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            vm.entitiesGrid = entitiesGrid;
            entitiesGridTransform.watch(entitiesGrid);
            entitiesGrid.setConfig(gridConfig);
            gridPagination.setGrid(entitiesGrid);
            vm.gridPagination = gridPagination;
            $scope.gridPagination = gridPagination;
            gridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);
            vm.allEntWatch = pubsub.subscribe("Acct.allEntChange", vm.clearGridSelections);
            vm.showEntWatch = pubsub.subscribe("Acct.showEntities", vm.showEntities);
            
            // vm.cmpChangeWatch = pubsub.subscribe("Acct.compChange", vm.setPropertiesByCompany);
            vm.gridSelectionWatch = vm.entitiesGrid.subscribe("selectChange", vm.gridRowSelectionChange);
            vm.gridSelectAllWatch = vm.entitiesGrid.subscribe("selectAll", vm.gridSelectAllChange);
            // vm.gridAllWatch = entitiesGrid.subscribe("selectAll", vm.selectAllEntities);
            vm.filterData = vm.entitiesGrid.subscribe("filterBy", vm.filter.bind(vm));

            if (persona.isReady()) {
                vm.loadData();
            } else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }

        };

        vm.showEntities = function (val) {
            vm.isShowEntities = val;
            logc("isAllProperties", vm.isAllProperties());
        };

        vm.clearGridSelections = function() {
            //clear selections, if theres any
            vm.entitiesGrid.selectAll(false);
            vm.entitiesGrid.updateSelected();
        };

        vm.isAllProperties = function() {
            return switchModel.getAllProperties();
        };

        vm.isUserHasManageProductAccess = function() {
            return !persona.data.hasManageAccountingProductAccess;
        };

        vm.filter = function(filterBy){
            vm.filteredRecords = $filter("filter")(vm.dataReq.records, filterBy);
        };

        vm.isActive = function() {
            return ADataModel.isActive();
        };

        vm.loadData = function() {
            if (persona.isReady() && ADataModel.isActive()) {
                entitiesGrid.busy(true);
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
            entitiesGrid.busy(false);
            if (resp.records && resp.records.length > 0) {
                switchModel.setProperties(resp.records);
                gridPagination.setData(resp.records).goToPage({
                    number: 0
                });

                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    vm.setViewUserState(resp);
                } else {
                    if (resp.additional && resp.additional.allProperties) {
                        vm.allProperties = resp.additional.allProperties;
                        vm.setAllProperties(true);
                    } else {
                        vm.allProperties = false;
                        ADataModel.setProperties(resp.records);
                    }
                }
            }
            if (resp.isError) {
                vm.isDataError = true;
                if (resp.errorReason !== "") {
                    vm.dataErrorReason = resp.errorReason;
                } else {
                    vm.dataErrorReason = genericDataErrorReason;
                }
            }
        };

        vm.setAllProperties = function(val) {
            if (val) {
                //clear selections, if theres any
                vm.entitiesGrid.selectAll(false);
                vm.entitiesGrid.updateSelected();
            } else {
                ADataModel.setProperties(vm.dataReq.records);
            }
        };

        vm.selectAllProperties = function (val) {
            ADataModel.setAllProperties(vm.dataReq.records, val);
        };

        vm.setViewUserState = function(data) {
            data.records.forEach(function(item) {
                angular.extend(item, {
                    disableSelection: false
                });
                item.disableSelection = true;
            });
            vm.allProperties = false;
            ADataModel.setProperties(data.records);
        };

        vm.setPropertiesByCompany = function(comp) {

            if (ADataModel.getProperties() != undefined && comp != undefined) {

                var records = $filter("filter")(ADataModel.getProperties(), {
                    companyId: comp.id
                }, true);

                records.forEach(function(item) {
                    item.isAssigned = comp.isAssigned;
                    item.disableSelection = comp.isAssigned;
                });

                vm.goToPage();

            }
        };

        vm.gridRowSelectionChange = function(val) {
            pubsub.publish("Acct.propChange", val);
        };

        vm.gridSelectAllChange = function(val) {
            vm.selectAllEntities(val);
            if (ADataModel.getCompanies() != undefined) {
                var checkAllCompanies = 0;
                
                ADataModel.getCompanies().forEach(function(comp) {

                    var propertiesByComp = $filter("filter")(ADataModel.getProperties(), {
                        companyId: comp.id,
                        isAssigned: val
                    }, true);

                    if (propertiesByComp.length > 0) {
                        var properties = $filter("filter")(propertiesByComp, {
                            isAssigned: val
                        }, true);

                        comp.isAssigned = val;             
                    }

                    if(comp.isAssigned){
                        checkAllCompanies++;
                    }
                });
                
                if(ADataModel.getCompanies().length === checkAllCompanies){
                    pubsub.publish("Acct.setAllCompaniesGridValue",true);
                }
                else if(checkAllCompanies === 0) {
                    pubsub.publish("Acct.setAllCompaniesGridValue",false);
                }
            }
        };

        vm.goToPage =function () {
            logc("page change");
            vm.gridPagination.setGridData();
        };

        vm.selectAllEntities = function (val) {
            //ADataModel.setAllEntities(vm.dataReq.records, val);
            if(vm.filteredRecords !== undefined){
                ADataModel.setAllEntities(vm.filteredRecords, val);
            }
            else{
                ADataModel.setAllEntities(vm.dataReq.records, val);
            }
        };

        vm.destroy = function() {
            vm.destWatch();
            vm.allEntWatch();
            vm.gridAllWatch();
            vm.showEntWatch();
            // vm.cmpChangeWatch();
            vm.gridSelectionWatch();
            vm.gridSelectAllWatch();

            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }
            entitiesGrid.destroy();
            entitiesGridTransform.destroy();
            gridPagination.destroy();
            entitiesGrid = undefined;
            entitiesGridTransform = undefined;
            gridPagination = undefined;
            vm = undefined;
            $scope = undefined;
            vm.filteredRecords = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("AEntitiesGridCtrl", [
            "$scope",
            "$filter",
            "AEntitiesSvc",
            "rpGridModel",
            "AEntitiesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "AccountingDataModel",
            "rpSwitchConfig",
            "userDetailsModel",
            "routeSecurity",
            "ASwitchModel",
            "pubsub",
            AEntitiesGridCtrl
        ]);
})(angular);

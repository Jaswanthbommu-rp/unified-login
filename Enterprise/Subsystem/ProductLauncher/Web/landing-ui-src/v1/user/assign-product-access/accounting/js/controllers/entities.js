//  Entities Grid Tab Controller

(function(angular, undefined) {
    "use strict";

    function AEntitiesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, ADataModel, switchConfig, userDetailsModel, security, switchModel, pubsub) {
        var vm = this,
            entitiesGrid = gridModel(),
            entitiesGridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function() {
            vm.allProperties = true;
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
            // vm.cmpChangeWatch = pubsub.subscribe("Acct.compChange", vm.setPropertiesByCompany);
            vm.gridSelectionWatch = vm.entitiesGrid.subscribe("selectChange", vm.gridRowSelectionChange);
            vm.gridSelectAllWatch = vm.entitiesGrid.subscribe("selectAll", vm.gridSelectAllChange);


            if (persona.isReady()) {
                vm.loadData();
            } else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }

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

            if (ADataModel.getCompanies() != undefined) {

                ADataModel.getCompanies().forEach(function(comp) {

                    var propertiesByComp = $filter("filter")(ADataModel.getProperties(), {
                        companyId: comp.id
                    }, true);

                    if (propertiesByComp.length > 0) {

                        var properties = $filter("filter")(propertiesByComp, {
                            isAssigned: true
                        }, true);

                        // if (properties.length > 0) {
                        //     comp.isAssigned = false;
                        // } else {
                        //     comp.isAssigned = true;
                        // }

                        if (properties.length > 0) {
                            comp.isAssigned = true;
                        }
                    }

                });

            }
        };

        vm.goToPage =function () {
            logc("page change");
            vm.gridPagination.setGridData();
        };



        vm.destroy = function() {
            vm.destWatch();
            vm.allEntWatch();
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

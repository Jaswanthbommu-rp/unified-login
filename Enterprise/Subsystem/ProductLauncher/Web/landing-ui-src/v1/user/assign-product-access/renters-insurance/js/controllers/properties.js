//  Properties Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function RentersInsurancePropertiesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, RentInsDataModel, userDetailsModel, security) {
        var vm = this,
            filteredRecords,
            propertiesGrid = gridModel(),
            propertiesGridTransform = gridTransformSvc(),
            propertiesGridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.corporateUser = false;
            vm.allProperties = false;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            vm.propertiesGrid = propertiesGrid;
            propertiesGridTransform.watch(propertiesGrid);
            propertiesGrid.setConfig(gridConfig);
            propertiesGridPagination.setGrid(propertiesGrid);
            $scope.propertiesGridPagination = propertiesGridPagination;
            propertiesGridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);
            vm.gridAllWatch = propertiesGrid.subscribe("selectAll", vm.selectAllProperties);
            vm.filterData = propertiesGrid.subscribe("filterBy", vm.filter.bind(vm));


            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }
        };

        vm.isActive = function () {
            return RentInsDataModel.isActive();
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageRentersInsuranceProductAccess;
        };
        vm.filter = function(filterBy){
            vm.filteredRecords = $filter("filter")(vm.dataReq.records, filterBy);
        };
    
        vm.loadData = function () {
            if (persona.isReady() && vm.isActive()) {
                propertiesGrid.busy(true);
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId()
                };

                vm.activeWatch();
                vm.personaWatch();
                vm.dataReq = dataSvc.get(params, vm.setData);
            }
        };

        vm.setData = function (resp) {
            propertiesGrid.busy(false);
            if (resp.records && resp.records.length > 0) {
                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    resp.records.forEach(function (item) {
                        angular.extend(item, {
                            disableSelection: false
                        });
                        item.disableSelection = true;
                    });
                }

                propertiesGridPagination.setData(resp.records).goToPage({
                    number: 0
                });

                RentInsDataModel.setProperties(resp.records);
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
        vm.selectAllProperties = function (val) {
            if(vm.filteredRecords !== undefined){
                RentInsDataModel.setAllPropertiesData(vm.filteredRecords, val);
            }
            else{
                RentInsDataModel.setAllPropertiesData(vm.dataReq.records, val);
            } 
        };

        vm.destroy = function () {
            vm.destWatch();
            propertiesGrid.destroy();
            propertiesGridTransform.destroy();
            propertiesGridPagination.destroy();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }

            vm = undefined;
            propertiesGrid = undefined;
            $scope = undefined;
            propertiesGridTransform = undefined;
            propertiesGridPagination = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("RentersInsurancePropertiesGridCtrl", [
            "$scope",
            "$filter",
            "RentersInsurancePropertiesSvc",
            "rpGridModel",
            "RIPropertiesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "rentersInsuranceDataModel",
            "userDetailsModel",
            "routeSecurity",
            RentersInsurancePropertiesGridCtrl
        ]);
})(angular);

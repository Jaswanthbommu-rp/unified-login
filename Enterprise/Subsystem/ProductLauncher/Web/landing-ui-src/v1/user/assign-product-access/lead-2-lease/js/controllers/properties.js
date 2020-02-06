//  Properties Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function Lead2LeasePropertiesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, lead2LeaseDataModel, userDetailsModel, security) {
        var vm = this,
            filteredRecords,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.allProperties = false;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            vm.grid = grid;
            gridTransform.watch(grid);
            grid.setConfig(gridConfig);
            gridPagination.setGrid(grid);
            $scope.gridPagination = gridPagination;
            gridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }

            vm.gridAllWatch = grid.subscribe("selectAll", vm.selectAllProperties);
            vm.filterData = grid.subscribe("filterBy", vm.filter.bind(vm));
        };

        vm.isActive = function () {
            return lead2LeaseDataModel.isActive();
        };
        vm.filter = function(filterBy){
            vm.filteredRecords = $filter("filter")(vm.dataReq.records, filterBy);
        };
        vm.loadData = function () {
            if (persona.isReady() && lead2LeaseDataModel.isActive()) {
                grid.busy(true);
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
            grid.busy(false);
            if (resp.records && resp.records.length > 0) {
                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    resp.records.forEach(function (item) {
                        angular.extend(item, {
                            disableSelection: false
                        });
                        item.disableSelection = true;
                    });
                }

                gridPagination.setData(resp.records).goToPage({
                    number: 0
                });

                lead2LeaseDataModel.setProperties(resp.records);
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

        vm.selectAllProperties = function(val){
            if(vm.filteredRecords !== undefined){
                lead2LeaseDataModel.setAllProperties(vm.filteredRecords, val);
            }
            else{
                lead2LeaseDataModel.setAllProperties(vm.dataReq.records, val);
            } 
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageLead2LeaseProductAccess;
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.gridAllWatch();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }
            grid.destroy();
            gridTransform.destroy();
            gridPagination.destroy();
            grid = undefined;
            gridTransform = undefined;
            gridPagination = undefined;
            vm = undefined;
            $scope = undefined;
            filteredRecords=undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("Lead2LeasePropertiesGridCtrl", [
            "$scope",
            "$filter",
            "lead2LeasePropertiesSvc",
            "rpGridModel",
            "lead2LeasePropertiesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "lead2LeaseDataModel",
            "userDetailsModel",
            "routeSecurity",
            Lead2LeasePropertiesGridCtrl
        ]);
})(angular);

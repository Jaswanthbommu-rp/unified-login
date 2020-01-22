//  Properties Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProspectContactCenterPropertiesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, prospectContactCenterDataModel, userDetailsModel, pubsub, security) {
        var vm = this,
            hasViewUserAccess,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "",
            activeProperties = [],
            inactiveProperties = [];

        vm.init = function () {
            vm.propertySelect = "active";
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
            
            vm.activeProperties = activeProperties;
            vm.inactiveProperties = inactiveProperties;

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }

            vm.updateWatch = pubsub.subscribe("pcc.properties-radio", vm.updateRecords);

            vm.filterData = vm.grid.subscribe("filterBy", vm.filter.bind(vm));
        };

        vm.filter = function(filterBy){            
            if(vm.propertySelect === 'active') {
                vm.filteredPropertiesArray = $filter("filter")(vm.activeProperties, filterBy);
            }
            else if(vm.propertySelect === 'inactive') {
                vm.filteredPropertiesArray = $filter("filter")(vm.inactiveProperties, filterBy);
            }

            gridPagination.setData(vm.filteredPropertiesArray).goToPage({
                number: 0
            });
        };

        vm.hasViewOnlyAccess = function () {
            vm.isUserHasViewOnlyAccess = security.isAllowed("viewUser") || vm.isUserHasManageProductAccess();
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasProspectContactCenterProductAccess;
        };

        vm.isActive = function () {
            return prospectContactCenterDataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && prospectContactCenterDataModel.isActive()) {
                vm.hasViewOnlyAccess();
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

        vm.updateRecords = function (record) {
            vm.dataReq.records.forEach(function (item) {
                item.isAssigned = item.id == record.id;
            });
        };

        vm.resetDataModel = function () {
            vm.setAllProperties(vm.propertySelect);
        };

        vm.setData = function (resp) {
            grid.busy(false);
            if (resp.records && resp.records.length > 0) {
                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    resp.records.forEach(function (item) {
                        item.disabled = true;
                    });
                }

                resp.records.forEach(function (property) {
                    if (property.active == 'true') {
                        vm.activeProperties.push(property);
                        if(property.isAssigned) {
                            vm.propertySelect = "active";
                        }
                    }
                    else if (property.active == 'false') {
                        vm.inactiveProperties.push(property);
                        if(property.isAssigned) {
                            vm.propertySelect = "inactive";
                        }
                    }
                });

                if (resp.additional && resp.additional.allProperties) {
                    vm.propertySelect = "all";
                     vm.setAllProperties(vm.propertySelect);
                }
                else {
                    vm.setAllProperties(vm.propertySelect);
                }
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

        vm.setAllProperties = function (val) {
            if(val == "active") {
                prospectContactCenterDataModel.setProperties(vm.activeProperties);
                gridPagination.setData(vm.activeProperties).goToPage({
                    number: 0
                });
                vm.grid.filtersModel.reset();
            }
            else if(val == "inactive") {
                prospectContactCenterDataModel.setProperties(vm.inactiveProperties);
                gridPagination.setData(vm.inactiveProperties).goToPage({
                    number: 0
                });
                vm.grid.filtersModel.reset();
            }
            else{
                var allPropertiesArray = [];
                allPropertiesArray.push("all");
                prospectContactCenterDataModel.setProperties(allPropertiesArray);

                //clear selections, if theres any
                vm.dataReq.records.forEach(function (property) {
                    if (property.isAssigned) {
                        property.isAssigned = false;
                    }
                });
            }
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.updateWatch();
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
            vm.filteredPropertiesArray = [];
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProspectContactCenterPropertiesGridCtrl", [
            "$scope",
            "$filter",
            "prospectContactCenterPropertiesSvc",
            "rpGridModel",
            "prospectContactCenterPropertiesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "prospectContactCenterDataModel",
            "userDetailsModel",
            "pubsub",
            "routeSecurity",
            ProspectContactCenterPropertiesGridCtrl
        ]);
})(angular);
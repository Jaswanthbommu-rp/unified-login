//  Properties Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProspectContactCenterPropertiesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, prospectContactCenterDataModel, userDetailsModel, pubsub, security) {
        var vm = this,
            hasViewUserAccess,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.propertySelect = "property";
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            // vm.hasViewUserAccess = security.isAllowed("viewUser") || vm.isUserHasManageProductAccess();
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

            vm.updateWatch = pubsub.subscribe("pcc.properties-radio", vm.updateRecords);
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
            if (vm.propertySelect === "all") {
                vm.setAllProperties(true);
            }
            else {
                vm.setAllProperties(false);
            }
        };

        vm.setData = function (resp) {
            grid.busy(false);
            if (resp.records && resp.records.length > 0) {
                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    resp.records.forEach(function (item) {
                        item.disabled = true;
                    });
                }

                gridPagination.setData(resp.records).goToPage({
                    number: 0
                });

                if (resp.additional && resp.additional.allProperties) {
                    vm.propertySelect = "all";
                    vm.setAllProperties(true);
                }
                else {
                    prospectContactCenterDataModel.setProperties(resp.records);
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
            if (val) {
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
            else {
                prospectContactCenterDataModel.setProperties(vm.dataReq.records);
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

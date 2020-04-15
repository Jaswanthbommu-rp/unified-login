//  Properties Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function UtilityManagementPropertiesGridCtrl($scope, $filter, dataSvc, dataGroupSvc, gridModel, gridConfig, gridGroupConfig, gridRegionConfig, gridTransformSvc, gridPaginationModel, pubsub, persona, UtilManDataModel, userDetailsModel, security) {
        var vm = this,
            isUserHasViewOnlyAccess = false,
            propertiesGrid = gridModel(),
            propertyGroupGrid = gridModel(),
            regionsGrid = gridModel(),
            propertiesGridTransform = gridTransformSvc(),
            propertyGroupGridTransform = gridTransformSvc(),
            regionsGridTransform = gridTransformSvc(),
            propertiesGridPagination = gridPaginationModel(),
            propertyGroupGridPagination = gridPaginationModel(),
            regionsGridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.propertySelect = "";
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            // vm.isUserHasViewOnlyAccess = security.isAllowed("viewUser") || vm.isUserHasManageProductAccess();
           
            vm.propertiesGrid = propertiesGrid;
            propertiesGridTransform.watch(propertiesGrid);
            propertiesGrid.setConfig(gridConfig);
            propertiesGridPagination.setGrid(propertiesGrid);
            $scope.propertiesGridPagination = propertiesGridPagination;
            propertiesGridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.propertyGroupGrid = propertyGroupGrid;
            propertyGroupGridTransform.watch(propertyGroupGrid);
            propertyGroupGrid.setConfig(gridGroupConfig);
            propertyGroupGridPagination.setGrid(propertyGroupGrid);
            $scope.propertyGroupGridPagination = propertyGroupGridPagination;
            propertyGroupGridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.regionsGrid = regionsGrid;
            regionsGridTransform.watch(regionsGrid);
            regionsGrid.setConfig(gridRegionConfig);
            regionsGridPagination.setGrid(regionsGrid);
            $scope.regionsGridPagination = regionsGridPagination;
            regionsGridPagination.setConfig({
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
        };

        vm.hasViewOnlyAccess =function () {
            vm.isUserHasViewOnlyAccess = security.isAllowed("viewUser") || vm.isUserHasManageProductAccess();
        };

        vm.isActive = function () {
            return UtilManDataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && vm.isActive()) {
                vm.hasViewOnlyAccess();
                propertiesGrid.busy(true);
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId()
                };

                vm.activeWatch();
                vm.personaWatch();
                vm.dataReq = dataSvc.get(params, vm.setData);
                vm.dataGroupReq = dataGroupSvc.get(params, vm.setGroupData);
                //vm.dataRegionReq = dataRegionSvc.get(params, vm.setRegionData);
            }
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageUtilityManagementProductAccess;
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

                vm.properties = resp.records;
                propertiesGridPagination.setData(vm.properties).goToPage({
                    number: 0
                });
                UtilManDataModel.setProperties(vm.properties);
                if (resp.additional) {
                    vm.setAccessType(resp.additional.accessType);
                    vm.resetDataModel();
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

        vm.setGroupData = function (resp) {
            if (resp.records && resp.records.length) {
                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    resp.records.forEach(function (item) {
                        angular.extend(item, {
                            disableSelection: false
                        });
                        item.disableSelection = true;
                    });
                }

                vm.propertyGroups = resp.records;
                propertyGroupGridPagination.setData(resp.records).goToPage({
                    number: 0
                });
                UtilManDataModel.setPropertyGroups(resp.records);
            }

            if (resp.additional) {
                vm.setAccessType(resp.additional.accessType);
            }
        };

        vm.setRegionData = function (resp) {
            if (resp.records && resp.records.length) {
                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    resp.records.forEach(function (item) {
                        angular.extend(item, {
                            disableSelection: false
                        });
                        item.disableSelection = true;
                    });
                }

                vm.propertyGroups = resp.records;
                regionsGridPagination.setData(resp.records).goToPage({
                    number: 0
                });
                UtilManDataModel.setRegions(resp.records);
            }

            if (resp.additional) {
                vm.setAccessType(resp.additional.accessType);
            }
        };

        vm.setAccessType = function (accessType) {
            if (accessType == "propertyGroup") {
                vm.propertySelect = "group";
            }
            else if (accessType == "portfolio") {
                vm.propertySelect = "portfolio";
            }
            else {
                vm.propertySelect = "property";
            }
        };

        vm.resetDataModel = function () {
            if (vm.propertySelect === "property") {
                vm.clearPropertyGroups();
            }
            else if (vm.propertySelect === "group") {
                vm.clearProperties();
            }
            else if (vm.propertySelect === "portfolio") {
                vm.clearProperties();
                vm.clearPropertyGroups();
                vm.setPortfolioProperties();
            }
        };

        vm.clearProperties = function () {
            vm.propertiesGrid.selectAll(false);
            vm.propertiesGrid.updateSelected();
        };

        vm.clearPropertyGroups = function () {
            vm.propertyGroupGrid.selectAll(false);
            vm.propertyGroupGrid.updateSelected();
        };

        vm.clearRegions = function () {
            vm.regionsGrid.selectAll(false);
            vm.regionsGrid.updateSelected();
        };

        vm.setPortfolioProperties = function () {
            if (vm.propertySelect === 'portfolio') {
                var allPropertiesArray = [];
                allPropertiesArray.push(-1);
                UtilManDataModel.setProperties(allPropertiesArray);
            }
            else {
                UtilManDataModel.setProperties(vm.properties);
            }
        };

        vm.destroy = function () {
            vm.destWatch();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }
            if (vm.dataGroupReq) {
                vm.dataGroupReq.$cancelRequest();
            }
            if (vm.dataRegionReq) {
                vm.dataRegionReq.$cancelRequest();
            }
            propertiesGrid.destroy();
            propertiesGridTransform.destroy();
            propertiesGridPagination.destroy();
            propertiesGrid = undefined;
            propertiesGridTransform = undefined;
            propertiesGridPagination = undefined;
            propertyGroupGrid.destroy();
            propertyGroupGridTransform.destroy();
            propertyGroupGridPagination.destroy();
            propertyGroupGrid = undefined;
            propertyGroupGridTransform = undefined;
            propertyGroupGridPagination = undefined;
            regionsGrid.destroy();
            regionsGridTransform.destroy();
            regionsGridPagination.destroy();
            regionsGrid = undefined;
            regionsGridTransform = undefined;
            regionsGridPagination = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("UtilityManagementPropertiesGridCtrl", [
            "$scope",
            "$filter",
            "UtilityManagementPropertiesSvc",
            "UtilityManagementPropertyGroupSvc",
            "rpGridModel",
            "UtilityManagementPropertiesGridConfig",
            "UtilityManagementPropertyGroupGridConfig",
            "UtilityManagementRegionsGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "utilityManagementDataModel",
            "userDetailsModel",
            "routeSecurity",
            UtilityManagementPropertiesGridCtrl
        ]);
})(angular);

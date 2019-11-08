//  Properties Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function VendCompPropertiesGridCtrl($scope, $filter, dataSvc, dataGroupSvc, gridModel, gridConfig, gridGroupConfig, gridTransformSvc, gridPaginationModel, pubsub, persona, VendCompDataModel, userDetailsModel, security) {
        var vm = this,
            propertiesGrid = gridModel(),
            propertyGroupGrid = gridModel(),
            propertyGroupGridTransform = gridTransformSvc(),
            propertiesGridTransform = gridTransformSvc(),
            propertiesGridPagination = gridPaginationModel(),
            propertyGroupGridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.propertySelect = "property";
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
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

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);
            vm.params = {};

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }

            vm.updateWatch = pubsub.subscribe("vc.property-group-radio", vm.updateGroupRecords);
        };

        vm.isActive = function () {
            return VendCompDataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && vm.isActive()) {
                propertiesGrid.busy(true);
                propertyGroupGrid.busy(true);
                vm.params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId()
                };

                vm.activeWatch();
                vm.personaWatch();
                vm.dataReq = dataSvc.get(vm.params, vm.setData);
            }
        };

        vm.isUserHasViewOnlyAccess = function () {
            return security.isAllowed("viewUser") || vm.isUserHasManageProductAccess();
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageVendorComplianceProductAccess;
        };

        vm.updateRecords = function (record) {
            vm.properties.forEach(function (item) {
                item.isAssigned = item.id == record.id;
            });
        };

        vm.updateGroupRecords = function (record) {
            vm.propertyGroups.forEach(function (item) {
                item.isAssigned = item.propertyGroupId == record.propertyGroupId;
            });
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
                VendCompDataModel.setPropertyGroups(resp.records);
            }

            if (resp.additional) {
                var accessType = resp.additional.accessType;

                if (accessType == "allProperties") {
                    vm.propertySelect = "all";
                }
                else if (accessType == "propertyGroup") {
                    vm.propertySelect = "group";
                }
                else {
                    vm.propertySelect = "property";
                }
            }
            propertiesGrid.busy(false);
            propertyGroupGrid.busy(false);
            vm.resetDataModel();
        };

        vm.setData = function (resp) {
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

                VendCompDataModel.setProperties(vm.properties);
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
            dataGroupSvc.get(vm.params, vm.setGroupData);
        };

        vm.resetDataModel = function () {
            pubsub.publish("vc.access-type-change", vm.propertySelect);
            if (vm.propertySelect === "property") {
                vm.clearPropertyGroups();
            }
            else if (vm.propertySelect === "group") {
                vm.clearProperties();
            }
            else {
                vm.clearProperties();
                vm.clearPropertyGroups();
            }
            vm.setAllProperties();
        };

        vm.clearProperties = function () {
            vm.propertiesGrid.selectAll(false);
            vm.propertiesGrid.updateSelected();
        };

        vm.clearPropertyGroups = function () {
            // Since the radio is custom type, the selection is ot getting cleared by selectAll method
            vm.propertyGroups.forEach(function (item) {
                item.isAssigned = false;
            });
            vm.propertyGroupGrid.selectAll(false);
            vm.propertyGroupGrid.updateSelected();
        };

        vm.setAllProperties = function () {
            if (vm.propertySelect === 'all') {
                var allPropertiesArray = [];
                allPropertiesArray.push(-1);
                VendCompDataModel.setProperties(allPropertiesArray);
            }
            else {
                VendCompDataModel.setProperties(vm.properties);
            }
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.updateWatch();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }
            if (vm.dataGroupReq) {
                vm.dataGroupReq.$cancelRequest();
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
            vm.propertySelect = undefined;
            vm.params = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("VendCompPropertiesGridCtrl", [
            "$scope",
            "$filter",
            "VendCompPropertiesSvc",
            "VendCompPropertyGroupSvc",
            "rpGridModel",
            "VendCompPropertiesGridConfig",
            "VendCompPropertyGroupGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "vendorComplianceDataModel",
            "userDetailsModel",
            "routeSecurity",
            VendCompPropertiesGridCtrl
        ]);
})(angular);

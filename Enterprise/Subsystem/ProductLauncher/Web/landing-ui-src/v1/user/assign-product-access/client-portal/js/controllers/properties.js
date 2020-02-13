//  Properties Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ClientPortalPropertiesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, pubsub, persona, ClientPortalDataModel, userDetailsModel, security, configModel) {
        var vm = this,
            hasViewUserAccess,
            propertiesGrid = gridModel(),
            propertiesGridTransform = gridTransformSvc(),
            propertiesGridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.propertySelect = "property";
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            // vm.hasViewUserAccess = security.isAllowed("viewUser") || vm.isUserHasManageProductAccess();
            console.log('PROPERTY');
            vm.propertiesGrid = propertiesGrid;
            propertiesGridTransform.watch(propertiesGrid);

            vm.config = configModel.getGridConfig()[0];
            // propertiesGrid.setConfig(gridConfig);
            propertiesGrid.setConfig(vm.config);
            propertiesGridPagination.setGrid(propertiesGrid);
            $scope.propertiesGridPagination = propertiesGridPagination;
            propertiesGridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.radioconfig = configModel.getRadioConfig()[0];
            logc("cnfg for radio prop" ,vm.radioconfig);

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }

            pubsub.subscribe("cp.property-radio", vm.updateRecords);
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageClientPortalProductAccess;
        };

        vm.isActive = function () {
            return ClientPortalDataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && vm.isActive()) {
                vm.hasViewUserAccess = security.isAllowed("viewUser") || vm.isUserHasManageProductAccess();
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

        vm.updateRecords = function (record) {
            var propertiesData = ClientPortalDataModel.getProperties();

            propertiesData.forEach(function (item) {
                item.isAssigned = item.id == record.id;
            });
        };

        vm.setData = function (resp) {
            propertiesGrid.busy(false);
            if (resp.records && resp.records.length > 0) {
                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    resp.records.forEach(function (item) {
                        angular.extend(item, {
                            disabled: false
                        });
                        item.disabled = true;
                    });
                }

                propertiesGridPagination.setData(resp.records).goToPage({
                    number: 0
                });

                if (resp.additional) {
                    var allProperties = resp.additional.allProperties;

                    if (allProperties) {
                        vm.propertySelect = "all";
                        ClientPortalDataModel.setAllProperty(true);
                    }
                    else {
                        vm.propertySelect = "property";
                        ClientPortalDataModel.setAllProperty(false);
                    }
                }
                ClientPortalDataModel.setProperties(resp.records);
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

        vm.resetDataModel = function () {
            vm.clearProperties();
            vm.resetProperties();
        };

        vm.clearProperties = function () {
            vm.dataReq.records.forEach(function (property) {
                if (property.isAssigned) {
                    property.isAssigned = false;
                }
            });
        };

        vm.resetProperties = function () {
            if (vm.propertySelect === 'all') {
                var allPropertiesArray = [];
                allPropertiesArray.push(-1);
                ClientPortalDataModel.setProperties(allPropertiesArray);
                ClientPortalDataModel.setAllProperty(true);
            }
            else {
                ClientPortalDataModel.setProperties(vm.dataReq.records);
                ClientPortalDataModel.setAllProperty(false);
            }
        };

        vm.setAllProperties = function () {
            if (vm.propertySelect === 'all') {
                var allPropertiesArray = [];
                allPropertiesArray.push(-1);
                ClientPortalDataModel.setProperties(allPropertiesArray);
            }
            else {
                ClientPortalDataModel.setProperties(vm.dataReq.records);
            }
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.personaWatch();
            propertiesGrid.destroy();
            propertiesGridTransform.destroy();
            propertiesGridPagination.destroy();
            propertiesGrid = undefined;
            propertiesGridTransform = undefined;
            propertiesGridPagination = undefined;

            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ClientPortalPropertiesGridCtrl", [
            "$scope",
            "$filter",
            "clientPortalPropertiesSvc",
            "rpGridModel",
            "clientPortalPropertiesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "clientPortalDataModel",
            "userDetailsModel",
            "routeSecurity",
            "ConfigModel",
            ClientPortalPropertiesGridCtrl
        ]);
})(angular);

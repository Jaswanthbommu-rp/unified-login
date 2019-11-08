//  Properties Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ILMLeadAnalyticsPropertiesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, pubsub, persona, ILMLADataModel, userDetailsModel, sync, security) {
        var vm = this,
            propertiesGrid = gridModel(),
            propertiesGridTransform = gridTransformSvc(),
            propertiesGridPagination = gridPaginationModel(),
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


            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }

            sync.setPropertySelectKey("isAssigned");
            vm.gridSelectionWatch = propertiesGrid.subscribe("selectChange", vm.selectionChange);
            vm.gridAllWatch = propertiesGrid.subscribe("selectAll", vm.selectionAll);
            vm.updateGridWatch = pubsub.subscribe("ilmla.updateGrids", vm.updateGrid);
        };

        vm.updateGrid = function () {
            vm.propertiesGrid.updateSelected();
        };

        vm.selectionAll = function () {
            sync.allPropertyToGroupSync();
        };

        vm.selectionChange = function (record) {
            if (record) {
                sync.propertyToGroupSync(record);
            }
        };

        vm.isActive = function () {
            return ILMLADataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && vm.isActive()) {
                propertiesGrid.busy(true);
                var params = {
                    productType: "LeadAnalytics",
                    subjectPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId()
                };

                vm.activeWatch();
                vm.personaWatch();
                vm.dataReq = dataSvc.get(params, vm.setData);
            }
        };

        vm.updateRecords = function (record) {
            vm.properties.forEach(function (item) {
                item.isAssigned = item.id == record.id;
            });
        };

        vm.setData = function (resp) {
            propertiesGrid.busy(false);
            if (resp.records && resp.records.length > 0) {
                sync.setPropertyList(resp.records);
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
                ILMLADataModel.setProperties(vm.properties);
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

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageILMLeasingAnalyticsProductAccess;
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.gridSelectionWatch();
            vm.gridAllWatch();
            vm.updateGridWatch();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }

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
        .controller("ILMLeadAnalyticsPropertiesGridCtrl", [
            "$scope",
            "$filter",
            "ProductPropertiesSvc",
            "rpGridModel",
            "ILMLeadAnalyticsPropertiesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "ilmLeadAnalyticsDataModel",
            "userDetailsModel",
            "syncManager",
            "routeSecurity",
            ILMLeadAnalyticsPropertiesGridCtrl
        ]);
})(angular);
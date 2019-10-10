//  Properties Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ILMLeadManagementPropertiesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, pubsub, persona, ILMLMDataModel, userDetailsModel, security) {
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

        };

        vm.isActive = function () {
            return ILMLMDataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && vm.isActive()) {
                propertiesGrid.busy(true);
                var params = {
                    productType: "LeadManagement",
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
                ILMLMDataModel.setProperties(vm.properties);
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
            return !persona.data.hasManageILMLeadManagemementProductAccess;
        };

        vm.destroy = function () {
            vm.destWatch();

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
        .controller("ILMLeadManagementPropertiesGridCtrl", [
            "$scope",
            "$filter",
            "ProductPropertiesSvc",
            "rpGridModel",
            "ILMLeadManagementPropertiesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "ilmLeadManagementDataModel",
            "userDetailsModel",
            "routeSecurity",
            ILMLeadManagementPropertiesGridCtrl
        ]);
})(angular);

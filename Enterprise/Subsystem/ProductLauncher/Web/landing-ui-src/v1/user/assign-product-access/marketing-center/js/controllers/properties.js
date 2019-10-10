//  Properties Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function MCPropertiesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, MCDataModel, userDetailsModel, security, switchConfig) {
        var vm = this,
            allProperties,
            propertiesGrid = gridModel(),
            propertiesGridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel();

        vm.init = function () {
            vm.propertiesGrid = propertiesGrid;
            vm.propertiesError = $filter("productPanelText")("panelError.generic");
            vm.allProperties = false;

            propertiesGridTransform.watch(propertiesGrid);
            propertiesGrid.setConfig(gridConfig);
            gridPagination.setGrid(propertiesGrid);
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

            
        };

        vm.isActive = function () {
            return MCDataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && MCDataModel.isActive()) {
                vm.allPropSwitch = switchConfig({
                    onChange: vm.setAllProperties,
                    disabled: security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()
                });
                propertiesGrid.busy(true);
                var params = {
                    editorPersonaId: persona.getId(),
                    userPersonaId: userDetailsModel.getPersonaId()
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

                gridPagination.setData(resp.records).goToPage({
                    number: 0
                });

                if (resp.additional && resp.additional.allProperties) {
                    vm.allProperties = resp.additional.allProperties;
                    vm.setAllProperties(true);
                }
                else {
                    MCDataModel.setProperties(resp.records);
                }
               // MCDataModel.setProperties(resp.records);
            }
            if (resp.isError) {
                vm.isPropertiesError = true;
            }
        };

        vm.setAllProperties = function (val) {
            if (val) {
                var allPropertiesArray = [];
                allPropertiesArray.push("all");
                MCDataModel.setProperties(allPropertiesArray);

                //clear selections, if theres any
                // vm.propertiesGrid.selectAll(false);
                vm.propertiesGrid.updateSelected();
            }
            else {
                MCDataModel.setProperties(vm.dataReq.records);
            }
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageMarketingCenterProductAccess;
        };

        vm.destroy = function () {
            vm.destWatch();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }
            propertiesGrid.destroy();
            propertiesGridTransform.destroy();
            gridPagination.destroy();
            propertiesGrid = undefined;
            propertiesGridTransform = undefined;
            gridPagination = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("MCPropertiesGridCtrl", [
            "$scope",
            "$filter",
            "MCPropertiesSvc",
            "rpGridModel",
            "MCPropertiesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "MarketingCenterDataModel",
            "userDetailsModel",
            "routeSecurity",
            "rpSwitchConfig",
            MCPropertiesGridCtrl
        ]);
})(angular);

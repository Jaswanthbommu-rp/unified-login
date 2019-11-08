//  Properties Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function OSPropertiesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, OSDataModel, switchConfig, userDetailsModel, security) {
        var vm = this,
            allProperties,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.grid = grid;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            vm.allProperties = false;

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

            
        };

        vm.isActive = function () {
            return OSDataModel.isActive();
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageOneSiteProductAccess;
        };

        vm.loadData = function () {
            if (persona.isReady() && vm.isActive()) {
                 vm.allPropSwitch = switchConfig({
                    onChange: vm.setAllProperties,
                    disabled: security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()
                });
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
                    vm.setViewUserState(resp);
                }
                else {
                    gridPagination.setData(resp.records).goToPage({
                        number: 0
                    });
                    if (resp.additional && resp.additional.allProperties) {
                        vm.allProperties = resp.additional.allProperties;
                        vm.setAllProperties(true);
                    }
                    else {
                        OSDataModel.setProperties(resp.records);
                    }
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
                OSDataModel.setProperties(allPropertiesArray);

                //clear selections, if theres any
                vm.grid.selectAll(false);
                vm.grid.updateSelected();
            }
            else {
                OSDataModel.setProperties(vm.dataReq.records);
            }
        };

        vm.setViewUserState = function (data) {
            data.records.forEach(function (item) {
                angular.extend(item, {
                    disableSelection: false
                });
                item.disableSelection = true;
            });
            vm.allProperties = false;
            gridPagination.setData(data.records).goToPage({
                number: 0
            });
            OSDataModel.setProperties(data.records);
        };

        vm.destroy = function () {
            vm.destWatch();
            grid.destroy();
            gridTransform.destroy();
            gridPagination.destroy();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }

            vm = undefined;
            grid = undefined;
            $scope = undefined;
            gridTransform = undefined;
            gridPagination = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("OSPropertiesGridCtrl", [
            "$scope",
            "$filter",
            "OSPropertiesSvc",
            "rpGridModel",
            "osPropertiesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "onesiteDataModel",
            "rpSwitchConfig",
            "userDetailsModel",
            "routeSecurity",
            OSPropertiesGridCtrl
        ]);
})(angular);

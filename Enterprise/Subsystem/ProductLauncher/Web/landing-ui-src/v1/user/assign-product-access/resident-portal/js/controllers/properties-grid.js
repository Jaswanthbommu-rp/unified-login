//  Properties Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function RPPropertiesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, RPDataModel, switchConfig, userDetailsModel, pubsub, userStatus, security) {
        var vm = this,
            allProperties,
            displayAllProperties,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.grid = grid;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            vm.allProperties = false;
            vm.displayAllProperties = false;
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
            vm.allPropWatch = pubsub.subscribe("resPort.allProperties", vm.setAllProperties);

            if (persona.isReady()) {               
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }

            
        };

        vm.isActive = function () {
            return RPDataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && vm.isActive()) {
                 vm.allPropSwitch = switchConfig({
                    onChange: vm.setAllProperties,
                    disabled: security.isAllowed("viewUser") || !persona.data.hasResidentPortalUserAccess
                });
                grid.busy(true);
                var userPersonaId = userDetailsModel.getPersonaId();

                if (persona.hasResidentPortalUserAccess() && !userDetailsModel.userExists()) {
                    userPersonaId = persona.getId();
                }
                var params = {
                    userPersonaId: userPersonaId,
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
                gridPagination.setData(resp.records).goToPage({
                    number: 0
                });

                if (security.isAllowed("viewUser") || !persona.data.hasResidentPortalUserAccess) {
                    vm.setViewUserState(resp);
                }
                else {
                    if (resp.additional && resp.additional.allProperties) {
                        vm.allProperties = resp.additional.allProperties;
                        vm.setAllProperties(true);
                    }
                    else {
                        RPDataModel.setProperties(resp.records);
                    }

                    if (resp.additional && angular.isDefined(resp.additional.displayAllProperties)) {
                        vm.displayAllProperties = resp.additional.displayAllProperties;
                    }

                    if (!userStatus.isSuperUser() && persona.hasResidentPortalUserAccess()) {
                        vm.displayAllProperties = false;
                        if (!userDetailsModel.userExists()) {
                            //vm.grid.selectAll(false);
                            resp.records.forEach(function (item) {
                                item.isAssigned = false;
                            });

                            vm.grid.updateSelected();
                        }
                        if (!vm.allProperties) {
                            RPDataModel.setProperties(resp.records);
                        }
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
            RPDataModel.propertiesReady = true;
            pubsub.publish("rp.properties-ready", true);
        };

        vm.setAllProperties = function (val) {
            vm.allProperties = val;
            if (val) {
                var allPropertiesArray = [];
                allPropertiesArray.push("all");
                //clear selections, if there are any
                vm.grid.selectAll(val);
                RPDataModel.setProperties(allPropertiesArray);
            }
            else {
                if (vm.dataReq) {
                    vm.dataReq.records.forEach(function (item) {
                        item.isAssigned = false;
                    });
                }

                RPDataModel.setProperties(vm.dataReq.records);
            }

            vm.grid.updateSelected();
        };

        vm.setViewUserState = function (data) {
            data.records.forEach(function (item) {
                angular.extend(item, {
                    disableSelection: false
                });
                item.disableSelection = true;
            });
            vm.allProperties = false;
            RPDataModel.setProperties(data.records);
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.allPropWatch();
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
        .controller("RPPropertiesGridCtrl", [
            "$scope",
            "$filter",
            "RPPropertiesSvc",
            "rpGridModel",
            "resportPropertiesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "residentPortalsDataModel",
            "rpSwitchConfig",
            "userDetailsModel",
            "pubsub",
            "userStatusModel",
            "routeSecurity",
            RPPropertiesGridCtrl
        ]);
})(angular);

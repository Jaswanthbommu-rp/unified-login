//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function CPCompRolesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, pubsub, persona, cpDataModel, userDetailsModel, security) {
        var vm = this,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            tabsDataAll = ["roles"], //,"llc","properties"
            tabsDataRolesOnly = ["roles"],
            // allProperties = false,
            genericDataErrorReason = "";

        vm.init = function () {
            vm.data = [];
            vm.productDisabled = false;
            vm.grid = grid;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            vm.panelName = $filter("productPanelText")("panelName.clickPay");
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
            vm.productAccessWatch = pubsub.subscribe("pa.regUserNoEmailNotAllowed", vm.setProductDisabled);

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }
            vm.cpCompWatch = pubsub.subscribe("clickpay.companies", vm.refreshGrid);
            vm.cpPropWatch = pubsub.subscribe("clickpay.properties", vm.refreshGrid);
            
            
        };

        vm.refreshGrid = function (items) {
            
            vm.data.forEach(function (role) {
                items.forEach(function (item) {

                    if(item.roleId === role.id){                       
                        role.orgsAssigned++;                       
                    }
                });
            });

            grid.updateSelected();
            
        };

        vm.setProductDisabled = function (value) {
            vm.productDisabled = value;
        };

        vm.isActive = function () {
            return cpDataModel.isActive();
        };

        vm.loadData = function () {            
            if (persona.isReady() && vm.isActive()) {

                grid.busy(true);
                var params = {
                    productType: "ClickPay",                    
                    editorPersonaId: persona.getId(),
                    subjectPersonaId: userDetailsModel.getPersonaId(),                    
                };

                vm.activeWatch();
                vm.personaWatch();
                vm.dataReq = dataSvc.get(params, vm.setData);
            }
        };

       
        vm.setData = function (resp) {
            
            vm.records = [];
            grid.busy(false);

            if (resp.records && resp.records.length > 0) {

                 if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {                    
                    var llc = [];
                    resp.records.forEach(function (item) {
                        angular.extend(item, {
                            disabled: false
                        });
                        item.disabled = true;

                    });
                 }

                cpDataModel.setNewUser(userDetailsModel.getPersonaId() === 0 ? true : false);

                vm.records = resp.records.map(function (role) {                    
                    angular.extend(role, {
                            orgTypeLink: "Assign " + vm.pascalCase(role.orgType)
                    });
                  
                    return role;
                });

                
                gridPagination.setData(vm.records).goToPage({
                    number: 0
                });

                cpDataModel.setRoles(vm.records);

                
                vm.setRoles(vm.records);
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

        vm.pascalCase = function (data) {
            return data.charAt(0).toUpperCase() + data.substr(1).toLowerCase();
        };
        
      
    

        vm.setRoles = function (data) {
            vm.data = data;
        };

        vm.setChanged = function () {            
            cpDataModel.setChanged();
        };

        
        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageClickPayProductAccess;
        };

        vm.destroy = function () {
            vm.destWatch();
            // vm.updateWatch();
            vm.personaWatch();
            vm.cpCompWatch();
            vm.cpPropWatch();
            vm.cpLlcWatch();
            vm.productAccessWatch();
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
        .controller("CPCompRolesGridCtrl", [
            "$scope",
            "$filter",
            "CPCompRolesSvc",
            "rpGridModel",
            "cpRolesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "clickPayProductAccessModel",
            "userDetailsModel",            
            "routeSecurity",            
            CPCompRolesGridCtrl
        ]);
})(angular);

//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function DepositAltRolesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, pubsub, persona, daDataModel, userDetailsModel, roleModel, security, tabsModel, $timeout) {
        var vm = this,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            tabsLoad = [ "roles", "properties", "notifications"],           
            allProperties = false,
            genericDataErrorReason = "";

        vm.init = function () {
            vm.records = [];
            vm.grid = grid;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
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

            vm.updateWatch = pubsub.subscribe("da.roles-radio", vm.setTabs);
        };

        vm.isActive = function () {
            return daDataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && vm.isActive()) {
                grid.busy(true);
                
                var params = {
                    productType: "DepositAlternative",                    
                    editorPersonaId: persona.getId(),
                    subjectPersonaId: userDetailsModel.getPersonaId(),                    
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
                    resp.records.forEach(function (item) {
                        angular.extend(item, {
                            disabled: false
                        });
                        item.disabled = true;                       
                    });
                }

            
                vm.records = resp.records.map(function (role) {     
                    role.roleType = "";               
                    return roleModel(role);
                });

                 
                gridPagination.setData(vm.records).goToPage({
                    number: 0
                });

               var selTab = {};
                if(userDetailsModel.getPersonaId() === 0){                    

                    var isAssigned =false;
                    vm.records.forEach(function(role) {                       
                        if (role.isAssigned) {
                           isAssigned = true;
                        }
                    });

                    vm.setNotification(true);   
                    if( !isAssigned ){
                        vm.records.forEach(function(role) {                       
                            if (role.id === 'agent') {
                               role.isAssigned = true;      
                               vm.setNotification(false); // for Agent Notification is false and tab is NOT visible                                                       
                            }
                        });
                    }                        
                                   
                }
                    
            

                vm.records.forEach(function(role) {                       
                    if (role.isAssigned ) {                       
                       selTab = role;                          
                    }
                });
                
                vm.setTabs(selTab);
                daDataModel.setRoles(vm.records);               
                vm.setNotification( userDetailsModel.getPersonaId() === 0 ? true : resp.additional.canReceiveMonthlyReport);  
                
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

        vm.setNotification = function (val) {
            
            daDataModel.data.inputJson.canReceiveMonthlyReport = val;
        };

        vm.setTabs = function (record) {

            var activeTabs = ["roles",  "notifications"];

            if( record.id ===  'property_manager'){
                activeTabs = [ "roles", "properties", "notifications"];     
                vm.setNotification(true);       
                daDataModel.clearRegions();
                daDataModel.clearAreas();    
            }
            else if( record.id === 'agent' ){
                activeTabs = [ "roles", "properties"]; 
                vm.setNotification(false);       
                daDataModel.clearRegions();
                daDataModel.clearAreas();    
            }
            else if (record.id ===  'area'){
                activeTabs = [ "roles", "areas", "notifications"];
                vm.setNotification(true);     
                daDataModel.clearRegions();
                daDataModel.clearProperties();              
            }            
            else if (record.id ===  'region' ){
                activeTabs = [ "roles", "regions", "notifications"];
                vm.setNotification(true);     
                daDataModel.clearAreas();
                daDataModel.clearProperties();
            }            
            else if (  record.id ===  'company'   || record.id ===  'admin' || record.id === 'collections_agent' || record.id === 'insurance_provider'){
                activeTabs = [ "roles",  "notifications"];
                vm.setNotification(true);     
                daDataModel.clearRegions();
                daDataModel.clearAreas();
                daDataModel.clearProperties();
            }else{
                activeTabs = [ "roles",  "notifications"];
                vm.setNotification(true);     
                daDataModel.clearRegions();
                daDataModel.clearAreas();
                daDataModel.clearProperties();                
            }         

            vm.records.forEach(function (role) {                   
                    role.isAssigned = record.id === role.id;     
            });

            tabsModel.setTabs(activeTabs);
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageDepositAlternativeProductAccess;
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.updateWatch();
            vm.personaWatch();
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
        .controller("DepositAltRolesGridCtrl", [
            "$scope",
            "$filter",
            "DARolesSvc",
            "rpGridModel",
            "daRolesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "depositAlternativeProductAccessModel",
            "userDetailsModel",
            "daRoleModel",
            "routeSecurity",
            "DepositAlternativeTabsModel",
            "$timeout",
            DepositAltRolesGridCtrl
        ]);
})(angular);

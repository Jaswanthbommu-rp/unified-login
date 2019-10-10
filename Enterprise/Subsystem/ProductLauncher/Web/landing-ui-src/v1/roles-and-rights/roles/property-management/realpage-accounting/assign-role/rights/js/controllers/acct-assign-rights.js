
//  rights Controller

(function(angular, undefined) {
    "use strict";

    function AcctAssignRightsCtrl(
        $scope,
        pubsub,
        gridConfig,
        model,
        dataSvc,
        gridPaginationModel,
        tabsManager,
        $q,
        saveSvc,
        user, 
        tabsContext,
        persona,
        $filter    
    ) {
        var vm = this;
        vm.isError = false;
        vm.init = function() {
            
            vm.model = model;
            vm.model.gridInit();
            tabsManager.registerTab({
                id: "01",
                ctrl: vm
            });

            gridConfig.setSrc(vm);
            vm.state = tabsManager.getTabState("01");
            
            vm.isPageActive = true;
            vm.isError = false;
            vm.formWatch = $scope.$watch("assignRightsForm", vm.setForm);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.loadData = function() {
            
            var params = {
                editorPersonaId: persona.getId(),                
                roleId: tabsContext.get().data.id,
                roleName : tabsContext.get().data.name
            };

            dataSvc.getData(params)
                .then(vm.setDataFromSvc, model.setDataErr);
        };

        vm.setDataFromSvc = function(data) {
            data = model.setDefaultTypeDisabled(data);
            model.setData(data);
            model.setExistAssignedData(data);
            //$scope.rpTrackFormChanges.setData(data.records);
            model.setGridPagination(data);
        };

        model.setDefaultTypeDisabled = function(data) {            
            if (model.roleData.data.roletype.toLowerCase() === "default") {
                data.records.forEach(function(item) {
                    angular.extend(item, { disableSelection: false });
                    item.disableSelection = true;
                });
            }
            return data;
        };

       
        vm.setForm = function(form) {
            if (form) {
                vm.form = form;
                vm.formWatch();
            }
        };

        vm.setSubmitted = function() {
            vm.form.$setSubmitted();
            return vm;
        };


        vm.onTabActive = function() {
            vm.loadData();
            
        };

        vm.checkIsSelected =function () {
            var isSel = model.checkIsSelected();
                if(isSel === true){
                    vm.setError(false);
                }
                else{
                    vm.setError(true);
                }
                return isSel;
        };

        vm.setError = function (val) {
          vm.isError = val;  
        };


        vm.onUpdate = function() {
            vm.isError = false;
            vm.isSvrError = false;
            var newAssigned = model.getNewAssignedData();
            var unAssigned = model.getUnAssignedData();
            var selRights = model.getSelectedRecords();
            vm.updateDeferred = $q.defer();

            var parm = {
                "editorPersonaId": persona.getId(),
                "roleId": model.roleData.data.id,
                "roleName": model.roleData.data.name
            };

            var input = {
                "rightsToAdd": vm.getInputObj(newAssigned),
                "rightsToRemove": vm.getInputObj(unAssigned)
            };
            
            if(input.rightsToAdd.length === 0 && input.rightsToRemove.length === 0  ){                
                vm.isError = true;
            }else{
                saveSvc.save(parm, input).$promise
                    .then(vm.onUpdateSuccess, vm.onUpdateError);
            }

            return vm.updateDeferred.promise;
        };

        vm.getInputObj = function(arrIds) {  
           var retArr = [];          
           
            arrIds.forEach(function (id) {
                var records = $filter("filter")(model.getData().records, {
                    id: id
                },true);

                if(records != undefined && records.length > 0){
                    retArr.push(records[0]);
                }
            });
            return retArr;
        };

        vm.onUpdateError = function(resp) {  
            
            if(resp.isError === true ){                    
                    vm.isSvrError = true;
                    pubsub.publish("settings.osaAssignRoleError", resp);
                }else{        
                    vm.saveError = true;
                    vm.updateDeferred.reject();
            }
        };

        vm.onUpdateSuccess = function(resp) {
            if (!angular.isUndefined(resp.errorReason) && resp.errorReason.trim().length === 0 && resp.isError === false) {
                vm.saveError = false;
                vm.form.$setUntouched();
                vm.updateDeferred.resolve();
            } else {                
                if (resp.errorReason.toLowerCase().indexOf('unable to assign rights') != -1) {
                    resp.errorReason = "Error assigning rights" ;
                }
                tabsManager.resetCounts();
                pubsub.publish("settings.osaAssignRoleError", resp);
            }
        };

        vm.hasSaveError = function() {
            return vm.saveError;
        };

        vm.isDirty = function() {
            return vm.form.$dirty;
        };

        vm.isValid = function() {
            return vm.form.$valid;
        };

        vm.destroy = function() {
            vm.destWatch();
            model.reset();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("AcctAssignRightsCtrl", [
            "$scope",
            "pubsub",
            "acctAssignRightsConfig",
            "acctAssignRightsModel",
            "acctAssignRightsSvc",
            "rpGridPaginationModel",
            "acctAssignRoleTabsManager",
            "$q",
            "acctAssignRightSavesvc",
            "userSessionModel",
            "acctAssignTabsContext",    
            "personaDetails",     
            "$filter",   
            AcctAssignRightsCtrl
        ]);
})(angular);
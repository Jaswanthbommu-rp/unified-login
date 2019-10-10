//  rights Controller

(function(angular, undefined) {
    "use strict";

    function AcctCloneRightsCtrl(
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
        cloneTabsContext,
        notifySvc,
        persona,
        $filter
    ) {
        var vm = this;
        vm.isError = false;
        vm.isSvrError = false;
        vm.init = function() {

            tabsManager.registerTab({
                id: "01",
                ctrl: vm
            });
            vm.model = model;
            vm.model.gridInit();
            gridConfig.setSrc(vm);
            vm.state = tabsManager.getTabState("01");
            vm.isError = false;
            vm.isSvrError = false;
            vm.isPageActive = true;

            vm.formWatch = $scope.$watch("cloneRightsTabForm", vm.setForm);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.loadData = function() {
            var params = {
                editorPersonaId: persona.getId(),                
                roleId: cloneTabsContext.get().data.id,
                roleName: cloneTabsContext.get().data.name
            };

            dataSvc.getData(params)
                .then(vm.setDataFromSvc, model.setDataErr);
        };

        vm.checkIsSelected = function() {
            var isSel = model.checkIsSelected();
            if (isSel === true) {
                vm.setError(false);
            } else {
                vm.setError(true);
            }
            return isSel;
        };

        vm.setError = function(val) {
            vm.isError = val;
        };

        vm.setForm = function(form) {
            if (form) {
                vm.form = form;
                vm.formWatch();
            }
        };

        vm.setDataFromSvc = function(data) {
            model.setData(data);
            model.setExistAssignedData(data);
            // $scope.rpTrackFormChanges.setData(data.records);
            model.setGridPagination(data);
        };

        vm.setSubmitted = function() {
            vm.form.$setSubmitted();
            return vm;
        };


        vm.onTabActive = function() {
            vm.loadData();
        };

        vm.onUpdate = function(cloneRole) {
            
            var newAssigned = model.getNewAssignedData();
            var unAssigned = model.getUnAssignedData();
         
            var selRights = model.getSelectedRecords();
            vm.updateDeferred = $q.defer();

            var parm = {
                "editorPersonaId": persona.getId(),
                "roleId": cloneRole.roleId.value,
                "roleName": cloneRole.roleName.value
            };

            var input = {                
                "rightsToAdd": vm.getInputObj(newAssigned),
                "rightsToRemove": vm.getInputObj(unAssigned)
            };


            saveSvc.save(parm, input).$promise
                .then(vm.onUpdateSuccess, vm.onUpdateError);

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

            vm.saveError = true;
            vm.updateDeferred.reject();
            pubsub.publish("osaSettings.cloneRoleError", resp);           
        };

        vm.onUpdateSuccess = function(resp) {

          if (!angular.isUndefined(resp.errorReason) && resp.errorReason.trim().length === 0 && resp.isError === false) {
                vm.saveError = false;
                vm.form.$setUntouched();
                vm.updateDeferred.resolve();
            } else {                
                if (resp.errorReason.toLowerCase().indexOf('unable to assign rights') != -1) {
                    resp.errorReason = "Role cloned - Error assigning rights" ;
                }
                tabsManager.resetCounts();
                pubsub.publish("osaSettings.cloneRoleError", resp);
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
        .controller("AcctCloneRightsCtrl", [
            "$scope",
            "pubsub",
            "acctCloneRightsConfig",
            "acctCloneRightsModel",
            "acctCloneRightsSvc",
            "rpGridPaginationModel",
            "acctCloneRoleTabsManager",
            "$q",
            "acctCloneRightsSaveSvc",
            "userSessionModel",
            "acctCloneTabsContext",
            "notificationService",
            "personaDetails",
            "$filter",
            AcctCloneRightsCtrl
        ]);
})(angular);
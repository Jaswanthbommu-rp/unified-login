//  rights Controller

(function(angular, undefined) {
    "use strict";

    function UserMgmtCloneRightsCtrl(
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
        persona
    ) {
        var vm = this;

        vm.init = function() {

            tabsManager.registerTab({
                id: "01",
                ctrl: vm
            });
            vm.model = model;
            vm.model.gridInit();
            gridConfig.setSrc(vm);
            vm.state = tabsManager.getTabState("01");


            vm.isPageActive = true;

            vm.formWatch = $scope.$watch("cloneRightsTabForm", vm.setForm);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.loadData = function() {            
            var params = {
                editorPersonaId: persona.getId(),
                partyId:  persona.data.organization.partyId,
                roleId: cloneTabsContext.get().data.id
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
            // model.setEditorWithNoRightDisabled(data);          
            model.setData(data);
            model.setExistAssignedData(data);
            // $scope.rpTrackFormChanges.setData(data.records);
            model.setGridPagination(data);
        };

        model.setEditorWithNoRightDisabled = function(data) {   
                data.records.forEach(function(item) {
                    if (item.isEditorHasRight === false) {
                        angular.extend(item, { disableSelection: false });
                        item.disableSelection = true;
                    }
                });
           
            return data;
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
            var unAssigned =[]; //model.getUnAssignedData();
            
            vm.updateDeferred = $q.defer();

            var parm = {
                "editorPersonaId": persona.getId(),
                "roleId": cloneRole.role
            };

            var input = {                
                "rightsToAdd": newAssigned,
                "rightsToDelete": unAssigned
            };


            saveSvc.save(parm, input).$promise
                .then(vm.onUpdateSuccess, vm.onUpdateError);

            return vm.updateDeferred.promise;
        };


        vm.onUpdateError = function(resp) {            
            vm.saveError = true;
            vm.updateDeferred.reject();
        };

        vm.onUpdateSuccess = function(resp) {    
            if (!angular.isUndefined(resp.errorReason) && resp.errorReason.trim().length === 0 && resp.isError === false) {
                vm.saveError = false;
                vm.form.$setUntouched();
                vm.updateDeferred.resolve();
            } else {
                pubsub.publish("settings.cloneRoleError", resp);
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
        .controller("UserMgmtCloneRightsCtrl", [
            "$scope",
            "pubsub",
            "userMgmtCloneRightsConfig",
            "userMgmtCloneRightsModel",
            "userMgmtCloneRightsSvc",
            "rpGridPaginationModel",
            "userMgmtCloneRoleTabsManager",
            "$q",
            "userMgmtCloneRightsSaveSvc",
            "userSessionModel",
            "userMgmtCloneTabsContext",
            "personaDetails",
            UserMgmtCloneRightsCtrl
        ]);
})(angular);
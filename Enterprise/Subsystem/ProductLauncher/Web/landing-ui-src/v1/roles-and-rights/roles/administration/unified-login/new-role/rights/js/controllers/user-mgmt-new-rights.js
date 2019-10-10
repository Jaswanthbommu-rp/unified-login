//  rights Controller

(function(angular, undefined) {
    "use strict";

    function UserMgmtNewRightsCtrl(
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
        persona
    ) {
        var vm = this;
        vm.isError = false;
        vm.init = function() {

            tabsManager.registerTab({
                id: "01",
                ctrl: vm
            });

            gridConfig.setSrc(vm);
            vm.state = tabsManager.getTabState("01");
            
            vm.model = model;
            vm.model.gridInit();

            vm.isPageActive = true;
            vm.isError = false;
            vm.formWatch = $scope.$watch("newRightsTabForm", vm.setForm);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.loadData = function() {
            
            var params = {
                editorPersonaId: persona.getId(),
                partyId:  persona.data.organization.partyId
            };

            dataSvc.getData(params)
                .then(vm.setDataFromSvc, model.setDataErr);
        };

        vm.setDataFromSvc = function(data) {
           // model.setEditorWithNoRightDisabled(data);
            model.setData(data);
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


        vm.setForm = function(form) {
            if (form) {
                vm.form = form;
                vm.formWatch();
            }
        };

        vm.setError = function (val) {
          vm.isError = val;  
        };

        vm.setSubmitted = function() {
            vm.form.$setSubmitted();
            return vm;
        };

        vm.onTabActive = function() {
            vm.loadData();
        };

        vm.onUpdate = function(newRole) {

            var newAssigned = model.getNewAssignedData();
            var selRights = model.getSelectedRecords();
            vm.updateDeferred = $q.defer();

            var parm = {
                "editorPersonaId": persona.getId(),
                "roleId": newRole.role,
            };

            var input = {
                // "rightsToAdd": selRights.selected,
                "rightsToAdd": newAssigned,
                "rightsToDelete": []
            };


            saveSvc.save(parm, input).$promise
                .then(vm.onUpdateSuccess, vm.onUpdateError);

            return vm.updateDeferred.promise;
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

        vm.onUpdateError = function(resp) {            
            vm.saveError = true;
            vm.updateDeferred.reject();
        };

        vm.onUpdateSuccess = function(resp) {            
            vm.saveError = false;
            vm.form.$setUntouched();
            vm.updateDeferred.resolve();
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
        .controller("UserMgmtNewRightsCtrl", [
            "$scope",
            "pubsub",
            "userMgmtNewRightsConfig",
            "userMgmtNewRightsModel",
            "userMgmtNewRightsSvc",
            "rpGridPaginationModel",
            "userMgmtNewRoleTabsManager",
            "$q",
            "userMgmtNewSaveRightsSvc",
            "userSessionModel",
            "personaDetails",
            UserMgmtNewRightsCtrl
        ]);
})(angular);
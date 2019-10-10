//  rights Controller

(function(angular, undefined) {
    "use strict";

    function OnesiteCloneRightsCtrl(
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
        persona
    ) {
        var vm = this;
        vm.isError = false;
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

            vm.isPageActive = true;

            vm.formWatch = $scope.$watch("cloneRightsTabForm", vm.setForm);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.loadData = function() {
            var params = {
                editorPersonaId: persona.getId(),
                assignedToRoleOnly: false,
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

        vm.onUpdate = function(newRole) {

            var newAssigned = model.getNewAssignedData();
            var unAssigned = model.getUnAssignedData();
         
            var selRights = model.getSelectedRecords();
            vm.updateDeferred = $q.defer();

            var parm = {
                "editorPersonaId": persona.getId(),
                "roleId": newRole.role.id
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
            notifySvc.notify({
                text: "Error: Unable to update rights",
                buttons: {
                    sticker: false
                },
                type: "error"
            });
        };

        vm.onUpdateSuccess = function(resp) {

            vm.saveError = false;
            vm.form.$setUntouched();
            vm.updateDeferred.resolve();

            notifySvc.notify({
                text: "Success: Updated rights successfully",
                buttons: {
                    sticker: false
                },
                type: "success"
            });
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
        .controller("OnesiteCloneRightsCtrl", [
            "$scope",
            "pubsub",
            "onesiteCloneRightsConfig",
            "onesiteCloneRightsModel",
            "onesiteCloneRightsSvc",
            "rpGridPaginationModel",
            "onesiteCloneRoleTabsManager",
            "$q",
            "onesiteCloneRightsSaveSvc",
            "userSessionModel",
            "onesiteCloneTabsContext",
            "notificationService",
            "personaDetails",
            OnesiteCloneRightsCtrl
        ]);
})(angular);
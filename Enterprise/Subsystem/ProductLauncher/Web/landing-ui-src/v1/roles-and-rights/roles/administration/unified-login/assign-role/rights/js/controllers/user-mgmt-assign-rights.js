//  rights Controller

(function(angular, undefined) {
    "use strict";

    function UserMgmtAssignRightsCtrl(
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
        persona
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
                partyId: persona.data.organization.partyId,
                roleId: tabsContext.get().data.id
            };

            dataSvc.getData(params)
                .then(vm.setDataFromSvc, model.setDataErr);
        };

        vm.setDataFromSvc = function(data) {
            data = model.setDefaultTypeDisabled(data);
            // data = model.setEditorWithNoRightDisabled(data);
            model.setData(data);
            
            model.setExistAssignedData(data);
            // $scope.rpTrackFormChanges.setData(data.records);
            model.setGridPagination(data);
        };

        model.setDefaultTypeDisabled = function(data) {   
                      
            if (model.roleData.data.roletype.toLowerCase() === "system" ) {
                data.records.forEach(function(item) {
                    angular.extend(item, { disableSelection: false });
                    item.disableSelection = true;
                });
            }
            return data;
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

        vm.setSubmitted = function() {
            vm.form.$setSubmitted();
            return vm;
        };


        vm.onTabActive = function() {
            vm.loadData();

        };

        vm.onUpdate = function(assignRole) {

            var newAssigned = model.getNewAssignedData();
            var unAssigned = model.getUnAssignedData();

           
            var selRights = model.getSelectedRecords();
            vm.updateDeferred = $q.defer();

            var parm = {
                "editorPersonaId": persona.getId(),
                "roleId": model.roleData.data.id,
            };

            var input = {
                // "rightsToAdd": selRights.selected,
                // "rightsToDelete": selRights.deselected
                "rightsToAdd": newAssigned,
                "rightsToDelete": unAssigned
            };


            saveSvc.save(parm, input).$promise
                .then(vm.onUpdateSuccess, vm.onUpdateError);

            return vm.updateDeferred.promise;
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
        .controller("UserMgmtAssignRightsCtrl", [
            "$scope",
            "pubsub",
            "userMgmtAssignRightsConfig",
            "userMgmtAssignRightsModel",
            "userMgmtAssignRightsSvc",
            "rpGridPaginationModel",
            "userMgmtAssignRoleTabsManager",
            "$q",
            "userMgmtAssignRightSavesvc",
            "userSessionModel",
            "userMgmtAssignTabsContext",
            "personaDetails",
            UserMgmtAssignRightsCtrl
        ]);
})(angular);
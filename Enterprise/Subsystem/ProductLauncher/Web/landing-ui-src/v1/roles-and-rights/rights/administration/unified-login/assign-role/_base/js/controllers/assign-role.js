//  Assign Role Controller

(function (angular, undefined) {
    "use strict";

    function UMAssignRolesToRightsCtrl(
        $scope,
        $rootScope,
        pubsub,
        gridConfig,
        model,
        dataSvc,
        gridPaginationModel,
        $q,
        roleConfig,
        saveSvc,
        user, tabsContext, aside,
        persona,
        security
    ) {
        var vm = this,
            triggerID = "UMAssignRolesToRightManager";
        vm.isError = false;

        vm.init = function () {
            vm.security = security;
            vm.isError = false;
            vm.model = model;
            vm.model.setRoleData();
            vm.model.gridInit();
            gridConfig.setSrc(vm);
            vm.roleConfig = roleConfig;
            vm.loadData();
            vm.formWatch = $scope.$watch("rightsForm", vm.setForm);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.appWatch = $rootScope.$on("rpAppStateChange", vm.onAppStateChange);
        };

        vm.hasAccess =  function (){
            return security.isAllowed("manageRoleRight")  && !persona.hasViewOnlySupportToolAccess();
        };

        vm.loadData = function () {

            var params = {
                editorPersonaId: persona.getId(),
                rightId: tabsContext.get().data.id,
                partyId: persona.data.organization.partyId
            };

            dataSvc.getData(params)
                .then(vm.setDataFromSvc, model.setDataErr);

        };

        vm.setDataFromSvc = function (data) {
            // data = model.setEditorWithNoRightDisabled(data);
            data = model.setDefaultTypeDisabled(data);
            model.setData(data);
            model.setExistAssignedData(data);
            // $scope.rpTrackFormChanges.setData(data.records);
            model.setGridPagination(data);
        };

        vm.setForm = function (form) {
            if (form) {
                vm.form = form;
                vm.formWatch();
            }
        };

        vm.checkIsSelected = function () {
            var isSel = model.checkIsSelected();
            if (isSel === true) {
                vm.setError(false);
            }
            else {
                vm.setError(true);
            }
            return isSel;
        };

        vm.setError = function (val) {
            vm.isError = val;
        };

        vm.setSubmitted = function () {
            vm.form.$setSubmitted();
            return vm;
        };

        vm.onAppStateChange = function (ev, evData) {
            if (!ev.defaultPrevented && evData.triggerID == triggerID) {
                evData.onContinue();
            }
        };

        vm.cancel = function () {
            $rootScope.$emit("rpAppStateChange", {
                triggerID: triggerID,
                onContinue: vm.onCancelContinue
            });
        };

        vm.onCancelContinue = function () {
            aside.hide();
        };

        vm.update = function () {

            // return vm.updateDeferred.promise;
            if (vm.checkIsSelected() === true) {

                var newAssigned = model.getNewAssignedData();
                var unAssigned = model.getUnAssignedData();


                var roles = model.getSelectedRecords();
                vm.updateDeferred = $q.defer();

                var parm = {
                    "editorPersonaId": persona.getId(),
                    "rightId": tabsContext.get().data.id,
                    "assignStatus": true
                };



                var input = {
                    // "rolesToAdd": roles.selected,
                    // "rolesToDelete": roles.deselected
                    "rolesToAdd": newAssigned,
                    "rolesToDelete": unAssigned
                };


                saveSvc.save(parm, input).$promise
                    .then(vm.onUpdateSuccess, vm.onUpdateError);

                return vm.updateDeferred.promise;
            }

        };


        vm.onUpdateSuccess = function (resp) {
            vm.saveError = false;
            vm.form.$setUntouched();
            aside.hide();
            vm.updateDeferred.resolve();
            pubsub.publish("umAssignRolesToRight.update");
        };

        vm.onUpdateError = function (resp) {
            vm.saveError = true;
            vm.updateDeferred.reject();
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.appWatch();
            vm.formWatch();
            model.reset();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("UMAssignRolesToRightsCtrl", [
            "$scope",
            "$rootScope",
            "pubsub",
            "umAssignRolesToRightsConfig",
            "umAssignRolesToRightsModel",
            "umAssignRolesToRightsSvc",
            "rpGridPaginationModel",
            "$q",
            "umAssignRolesToRightsFormConfig",
            "umAssignRolesToRightsSavesvc",
            "userSessionModel",
            "umAssignRolesToRightsContext",
            "umAssignRolesToRightsAside",
            "personaDetails",
            "routeSecurity",
            UMAssignRolesToRightsCtrl
        ]);
})(angular);

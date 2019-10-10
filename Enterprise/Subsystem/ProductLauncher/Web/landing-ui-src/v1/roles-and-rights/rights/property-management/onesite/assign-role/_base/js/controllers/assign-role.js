//  Assign Role Controller

(function (angular, undefined) {
    "use strict";

    function AssignRolesToRightsCtrl(
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
        user, tabsContext, aside, persona, security
    ) {
        var vm = this,
            triggerID = "AssignRolesToRightManager";
        vm.isError = false;

        vm.init = function () {
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

        vm.hasAccess = function () {
            return security.isAllowed("manageRoleRight") && !persona.hasViewOnlySupportToolAccess();
        };

        vm.loadData = function () {

            var params = {
                editorPersonaId: persona.getId(),
                rightId: tabsContext.get().data.id,
                assignedOnly: false,
            };

            dataSvc.getData(params)
                .then(vm.setDataFromSvc, model.setDataErr);

        };

        vm.setDataFromSvc = function (data) {
            data = model.setDefaultTypeDisabled(data);
            model.setData(data);

            model.setExistAssignedData(data);
            //$scope.rpTrackFormChanges.setData(data.records);
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

            if (vm.checkIsSelected() === true) {

                var roles = model.getSelectedRecords();
                vm.updateDeferred = $q.defer();

                var newAssigned = model.getNewAssignedData();
                var unAssigned = model.getUnAssignedData();

                vm.updateSelectedRoles(newAssigned);
                vm.updateDeselectedRoles(unAssigned);

                return vm.updateDeferred.promise;
            }

        };

        vm.updateSelectedRoles = function (roles) {

            if (roles.length > 0) {
                var parmRolesAdded = {
                    "editorPersonaId": persona.getId(),
                    "rightId": tabsContext.get().data.id,
                    "assignStatus": true
                };

                var inputAdded = roles;

                saveSvc.save(parmRolesAdded, inputAdded).$promise
                    .then(vm.onUpdateSuccess, vm.onUpdateError);
            }
        };

        vm.updateDeselectedRoles = function (roles) {
            if (roles.length > 0) {

                var parmRolesRemoved = {
                    "editorPersonaId": persona.getId(),
                    "rightId": tabsContext.get().data.id,
                    "assignStatus": false
                };

                var inputRemoved = roles;

                saveSvc.save(parmRolesRemoved, inputRemoved).$promise
                    .then(vm.onUpdateSuccess, vm.onUpdateError);
            }
        };

        vm.onUpdateSuccess = function (resp) {
            vm.saveError = false;
            vm.form.$setUntouched();
            aside.hide();
            vm.updateDeferred.resolve();
            pubsub.publish("assignRolesToRight.update");
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
        .controller("AssignRolesToRightsCtrl", [
            "$scope",
            "$rootScope",
            "pubsub",
            "assignRolesToRightsConfig",
            "assignRolesToRightsModel",
            "assignRolesToRightsSvc",
            "rpGridPaginationModel",
            "$q",
            "assignRolesToRightsFormConfig",
            "assignRolesToRightsSavesvc",
            "userSessionModel",
            "assignRolesToRightsContext",
            "assignRolesToRightsAside",
            "personaDetails",
             "routeSecurity",
            AssignRolesToRightsCtrl
        ]);
})(angular);

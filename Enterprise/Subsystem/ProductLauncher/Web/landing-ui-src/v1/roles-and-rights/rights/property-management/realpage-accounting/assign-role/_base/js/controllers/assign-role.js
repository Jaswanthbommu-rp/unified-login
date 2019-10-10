//  Assign Role Controller

(function (angular, undefined) {
    "use strict";

    function AcctAssignRolesToRightsCtrl(
        $scope,
        $rootScope,
        $q,
        $filter,
        pubsub,
        gridConfig,
        model,
        dataSvc,
        gridPaginationModel,
        roleConfig,
        saveSvc,
        user,
        tabsContext,
        aside,
        persona,
        security
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
                "right": tabsContext.get().data
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
            var isSel = true; //model.checkIsSelected(); // accounting can have roles without rights
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

                // vm.updateSelectedRoles(newAssigned);
                // vm.updateDeselectedRoles(unAssigned);
                vm.updateRoles(newAssigned, unAssigned);
                return vm.updateDeferred.promise;
            }

        };



        vm.updateRoles = function (newAssigned, unAssigned) {
            vm.isError = false;

            var parmRolesAdded = {
                "editorPersonaId": persona.getId(),
                "rightId": tabsContext.get().data.id,
                "assignStatus": true,
                "right": tabsContext.get().data
            };

            var input = {
                "RolesToAdd": vm.getInputObj(newAssigned),
                "RolesToDelete": vm.getInputObj(unAssigned)
            };

            // if(input.RolesToAdd.length === 0 && input.RolesToDelete.length === 0  ){

            //     vm.isError = true;
            // }else{
            saveSvc.save(parmRolesAdded, input).$promise
                .then(vm.onUpdateSuccess, vm.onUpdateError);
            // }

        };



        vm.getInputObj = function (arrIds) {
            var retArr = [];

            arrIds.forEach(function (id) {
                var records = $filter("filter")(model.getData().records, {
                    id: id
                }, true);

                if (records != undefined && records.length > 0) {
                    retArr.push(records[0]);
                }
            });
            return retArr;
        };

        vm.onUpdateSuccess = function (resp) {
            vm.saveError = false;
            vm.form.$setUntouched();
            aside.hide();
            vm.updateDeferred.resolve();
            pubsub.publish("acctAssignRolesToRight.update");
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
        .controller("AcctAssignRolesToRightsCtrl", [
            "$scope",
            "$rootScope",
            "$q",
            "$filter",
            "pubsub",
            "acctAssignRolesToRightsConfig",
            "acctAssignRolesToRightsModel",
            "acctAssignRolesToRightsSvc",
            "rpGridPaginationModel",
            "acctAssignRolesToRightsFormConfig",
            "acctAssignRolesToRightsSavesvc",
            "userSessionModel",
            "acctAssignRolesToRightsContext",
            "acctAssignRolesToRightsAside",
            "personaDetails",
            "routeSecurity",
            AcctAssignRolesToRightsCtrl
        ]);
})(angular);

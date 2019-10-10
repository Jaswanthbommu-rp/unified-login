//  rights Controller

(function(angular, undefined) {
    "use strict";

    function AcctNewRightsCtrl(
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
        persona,
        $filter
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
                assignedToRoleOnly: false
            };

            dataSvc.getData(params)
                .then(vm.setDataFromSvc, model.setDataErr);
        };

        vm.onPageChange = function(data) {
            logc(data);
        };

        vm.setDataFromSvc = function(data) {
            model.setData(data);
            //$scope.rpTrackFormChanges.setData(data.records);
            model.setGridPagination(data);
        };

        vm.setForm = function(form) {
            if (form) {
                vm.form = form;
                vm.formWatch();
            }
        };

        vm.setError = function(val) {
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
            vm.isError = false;
            var newAssigned = model.getNewAssignedData();


            var selRights = model.getSelectedRecords();
            vm.updateDeferred = $q.defer();

            var parm = {
                "editorPersonaId": persona.getId(),
                "roleId": newRole.roleId.value,
                "roleName": newRole.roleName.value
            };

            var input = {
                "rightsToAdd": vm.getInputObj(newAssigned),
                "rightsToRemove": []
            };

            if (input.rightsToAdd.length === 0) {                
                vm.isError = true;
            } else {
                saveSvc.save(parm, input).$promise
                    .then(vm.onUpdateSuccess, vm.onUpdateError);
            }

            return vm.updateDeferred.promise;
        };

        vm.getInputObj = function(arrIds) {
            var retArr = [];

            arrIds.forEach(function(id) {
                var records = $filter("filter")(model.getData().records, {
                    id: id
                }, true);

                if (records != undefined && records.length > 0) {
                    retArr.push(records[0]);
                }
            });
            return retArr;
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


        vm.onUpdateError = function(resp) {
            vm.saveError = true;
            vm.updateDeferred.reject();
            pubsub.publish("osaSettings.newRoleError", resp);
        };

        vm.onUpdateSuccess = function(resp) {

            if (!angular.isUndefined(resp.errorReason) && resp.errorReason.trim().length === 0 && resp.isError === false) {
                vm.saveError = false;
                vm.form.$setUntouched();
                vm.updateDeferred.resolve();
            } else {                
                if (resp.errorReason.toLowerCase().indexOf('unable to assign rights') != -1) {
                    resp.errorReason = "Role created - Error assigning rights" ;
                }
                tabsManager.resetCounts();
                pubsub.publish("osaSettings.newRoleError", resp);
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
        .controller("AcctNewRightsCtrl", [
            "$scope",
            "pubsub",
            "acctNewRightsConfig",
            "acctNewRightsModel",
            "acctNewRightsSvc",
            "rpGridPaginationModel",
            "acctNewRoleTabsManager",
            "$q",
            "acctNewSaveRightsSvc",
            "userSessionModel",
            "personaDetails",
            "$filter",
            AcctNewRightsCtrl
        ]);
})(angular);
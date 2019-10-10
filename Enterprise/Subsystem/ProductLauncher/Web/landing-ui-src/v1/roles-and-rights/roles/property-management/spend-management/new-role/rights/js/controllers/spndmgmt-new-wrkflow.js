//  rights Controller

(function(angular, undefined) {
    "use strict";

    function SpndMgmtNewWrkFlCtrl(
        $scope,
        pubsub,
        // gridConfig,
        model,
        dataSvc,
        // gridPaginationModel,
        tabsManager,
        $q,
        saveSvc,
        user,
        persona,
        wfConfig,
        wfModel
    ) {
        var vm = this;
        vm.isError = false;

        vm.init = function() {

            tabsManager.registerTab({
                id: "02",
                ctrl: vm
            });
            vm.wfConfig = wfConfig;
            
            vm.state = tabsManager.getTabState("02");

            
            vm.model = wfModel;
            //vm.model.gridInit();

            vm.isPageActive = true;
            vm.isError = false;


            vm.formWatch = $scope.$watch("newWfTabForm", vm.setForm);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.loadData = function() {

            // var params = {
            //     editorPersonaId: persona.getId(),
            //     assignedToRoleOnly: false
            // };

            // dataSvc.getData(params)
            //     .then(vm.setDataFromSvc, model.setDataErr);
        };

        vm.onPageChange = function(data) {
            // logc(data);
        };

        vm.setDataFromSvc = function(data) {
            model.setData(data);            
        };

        vm.setForm = function(form) {
            if (form) {
                vm.form = form;
                vm.formWatch();
            }
        };

        vm.isChecked = function (val) {            
          return val === true ? "checked" : "";  
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

            vm.updateDeferred = $q.defer();           

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
            wfModel.reset();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("SpndMgmtNewWrkFlCtrl", [
            "$scope",
            "pubsub",
            // "spndmgmtNewRightsConfig",
            "spndmgmtNewRightsModel",
            "spndMgmtNewRightsSvc",
            // "rpGridPaginationModel",
            "spndMgmtNewRoleTabsManager",
            "$q",
            "spndMgmtNewSaveRightsSvc",
            "userSessionModel",
            "personaDetails",
            "spndmgmtNewRoleWrkFloFormConfig",
            "spndmgmtNewRoleWfModel",
            SpndMgmtNewWrkFlCtrl
        ]);
})(angular);
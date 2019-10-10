//  res-app goals  Tab Controller

(function(angular, undefined) {
    "use strict";

    function ResAppGoalsCtrl($scope, $filter, dataSvc, persona, UMDataModel, userDetailsModel, goalsConfig) {
        var vm = this;

        vm.init = function() {            
            vm.UMDataModel = UMDataModel;
            vm.goalsConfig = goalsConfig;
            goalsConfig.setMethodsSrc(vm);
            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);
            vm.formWatch = $scope.$watch("goalsForm", vm.setForm);
                        
            if (persona.isReady()) {
                vm.loadData();
            } else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }

        };

        vm.isActive = function() {
            return UMDataModel.isActive();
        };

        vm.loadData = function() {

            if (persona.isReady() && vm.isActive()) {
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId(),
                    partyId: persona.data.organization.partyId
                };

                vm.activeWatch();
                vm.personaWatch();
                vm.dataReq = dataSvc.get(params, vm.setData);
            }
        };

        vm.goalsChange = function () {
            UMDataModel.setGoalsValid(vm.isValid());
        };

        vm.setForm = function(form) {            
            if (form) {
                vm.form = form;                
                vm.form.$setSubmitted();
            }
        };

        vm.isValid = function () {
            return vm.form.$valid;
        };


        vm.setData = function(resp) {            
            if (resp.records && resp.records.length > 0) {
                UMDataModel.setRoles(resp.records);
            }

            if (resp.isError) {
                vm.isRolesError = true;
            }
        };

        vm.destroy = function() {
            vm.destWatch();
            vm.personaWatch();
            vm.formWatch();
            
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }

            vm = undefined;
            $scope = undefined;

        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ResAppGoalsCtrl", [
            "$scope",
            "$filter",
            "resAppUserRolesSvc",            
            "personaDetails",
            "resAppDataModel",
            "userDetailsModel",            
            "resAppGoalsFormConfig",
            ResAppGoalsCtrl
        ]);
})(angular);
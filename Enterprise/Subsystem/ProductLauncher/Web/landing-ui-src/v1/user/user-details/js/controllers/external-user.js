// 

(function (angular, undefined) {
    "use strict";

    function ExternalUserModalCtrl($scope, $location, modal, pubsub, persona, model, formConfig, $window, chkEmailModel) {
        var vm = this;

        vm.init = function () {            
            
            vm.destWatch = $scope.$on("$destroy", vm.destroy);            
            vm.model = model;
        };

        vm.dismissModal = function () {                 
            vm.setClearLoginName();                          
            vm.setFocus();          
            modal.hide();           
        };

        vm.setFocus = function (bool) {
           var inp = $window.document.getElementById('loginName');                   
           inp.focus();             
        };

        vm.selectUser = function () {   
            chkEmailModel.setIsBusy(false);              
            model.setUserTypeDefConfig(405);       

            pubsub.publish("settings.resetUserTypeOptions");
            vm.setData();
            vm.setFirstNameDisabled(true);
            vm.setMiddleNameDisabled(true);
            vm.setLastNameDisabled(true);
            vm.setLoginNameDisabled(true);
            
            pubsub.publish("settings.3rdParty");
            modal.hide();
        };

        vm.setData = function (bool) {
            model.data.firstName = model.externalUserData.person.firstName;
            model.data.lastName =  model.externalUserData.person.lastName;
            model.data.middleName = model.externalUserData.person.middleName;
        };

        vm.setFirstNameDisabled = function (bool) {
            formConfig.setFirstNameDisabled(bool);
        };

        vm.setMiddleNameDisabled = function (bool) {
            formConfig.setMiddleNameDisabled(bool);
        };

        vm.setLastNameDisabled = function (bool) {
            formConfig.setLastNameDisabled(bool);
        };

        vm.setLoginNameDisabled = function (bool) {
            formConfig.setLoginNameDisabled(bool);
        };

        vm.setClearLoginName = function () {
            model.clearLoginName();
        };

        

       
        vm.destroy = function () {
            vm.destWatch();           
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ExternalUserModalCtrl", [
            "$scope",
            "$location",
            "externalUserModal",           
            "pubsub",
            "personaDetails",
            "userDetailsModel",
            "userDetailsFormConfig",
            "$window",
            "chkEmailModel",
            ExternalUserModalCtrl
        ]);
})(angular);

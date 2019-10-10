 

(function (angular, undefined) {
    "use strict";

    function ExistingUserNoEmailModalCtrl($scope, $location, modal, pubsub, persona, model, formConfig, $window, chkEmailModel) {
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
        .controller("ExistingUserNoEmailModalCtrl", [
            "$scope",
            "$location",
            "existingNoEmailUserModal",   
            "pubsub",
            "personaDetails",
            "userDetailsModel",
            "userDetailsFormConfig",
            "$window",
            "chkEmailModel",
            ExistingUserNoEmailModalCtrl
        ]);
})(angular);

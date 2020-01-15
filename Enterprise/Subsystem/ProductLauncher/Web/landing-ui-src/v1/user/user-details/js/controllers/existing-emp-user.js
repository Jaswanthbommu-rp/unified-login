// 

(function (angular, undefined) {
    "use strict";

    function ExistingEmpUserModalCtrl($scope, $location, modal, pubsub, persona, model, $window) {
        var vm = this;

        vm.init = function () {                        
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.showEmpExistingUser = function () {     
            modal.hide();
            $window.location.href = model.getExistingUserLink();               
        };  

        vm.getShowEmpExistingUserLink = function () {     
            return model.getShowExistingUserLink();               
        };

        vm.dismissModal = function () {
           var inp = $window.document.getElementById('loginName');
            vm.setClearLoginName(); 
            inp.focus();   
            inp.select();        
            modal.hide(); 
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
        .controller("ExistingEmpUserModalCtrl", [
            "$scope",
            "$location",
            "existingEmpUserModal",           
            "pubsub",
            "personaDetails",
            "userDetailsModel",
            "$window",
            ExistingEmpUserModalCtrl
        ]);
})(angular);

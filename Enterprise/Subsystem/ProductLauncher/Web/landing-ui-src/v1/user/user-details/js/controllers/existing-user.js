// 

(function (angular, undefined) {
    "use strict";

    function ExistingUserModalCtrl($scope, $location, modal, pubsub, persona, model, $window) {
        var vm = this;

        vm.init = function () {                        
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.showExistingUser = function () {     
            modal.hide();
            $window.location.href = model.getExistingUserLink();               
        };  

        vm.getShowExistingUserLink = function () {     
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
        .controller("ExistingUserModalCtrl", [
            "$scope",
            "$location",
            "existingUserModal",           
            "pubsub",
            "personaDetails",
            "userDetailsModel",
            "$window",
            ExistingUserModalCtrl
        ]);
})(angular);

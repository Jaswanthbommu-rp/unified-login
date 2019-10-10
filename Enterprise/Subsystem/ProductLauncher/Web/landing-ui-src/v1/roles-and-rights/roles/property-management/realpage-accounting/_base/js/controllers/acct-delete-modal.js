//    delete roles Controller

(function (angular, undefined) {
    "use strict";

    function AcctDeleteModalCtrl($scope, model, dataSvcDel) {
        var vm = this;

        vm.init = function () {                     
            vm.model = model;  
            vm.destWatch = $scope.$on("$destroy", vm.destroy);            
        };

        vm.hideModal = function () {
            model.hideDeleteRolesConfirmModal();
        };

        vm.hideDeleteRoleModal = function () {
            model.hideDeleteRoleConfirmModal();
        };    

        vm.hideWarningModal = function () {
            model.hideWarningModal();
        };   

         vm.hideDefWarningModal = function () {
            model.hideDefWarningModal();
        };   
        
        vm.hideNotSelectedModal = function () {
            model.hideNotSelectedModal();
        };

        vm.confirmDeleteSelectedRoles = function () {            
            vm.hideModal();            
            model.deleteSelectedRoles(dataSvcDel);
        };

        vm.confirmDeleteRoleModal = function () {            
            vm.hideDeleteRoleModal();  
            var role = model.getDelRole();          
            model.deleteRole(role, dataSvcDel);
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
        .controller("AcctDeleteModalCtrl", [
            "$scope",
            "acctRolesModel",
            "acctDeleteRoleSvc",
            AcctDeleteModalCtrl
        ]);
})(angular);

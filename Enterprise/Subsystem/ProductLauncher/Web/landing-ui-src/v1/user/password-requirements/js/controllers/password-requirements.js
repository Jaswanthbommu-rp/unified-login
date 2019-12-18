//  Password Requirements Controller

(function (angular, undefined) {
    "use strict";

    function PassReqCtrl($scope, model, session, PasswordPolicySvc) {
        var vm = this;

        vm.init = function () {
            vm.formState = {
                password: null,
                isSaving: false
            };
            vm.validatePasswordPolicy();
            vm.model = model;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.validatePasswordPolicy = function() {
            var records = session.getOrganization();
            var params = {
                    PartyId: records[0].partyId
            };
            
            PasswordPolicySvc.get(params, vm.onResponseReady, vm.setDataErr);
        };

        vm.onResponseReady = function (resp) {
                var settings = resp.data;
                vm.formState.password = model.init(settings);
                $scope.$digest();

        };

        vm.setDataErr = function (resp) {
            alert(resp);
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
        .controller("PassReqCtrl", ["$scope", "passReqModel","userSessionModel","PasswordPolicySvc", PassReqCtrl]);
})(angular);

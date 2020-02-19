//  UserMgmt Role Info Icon Controller

(function(angular, undefined) {
    "use strict";

    function EntitiesInfoCtrl($scope, aside) {
        var vm = this;

        vm.init = function() {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.showAside = function(record) {
            logc('showing aside model');
            //rightsModel.setName(record.name);
            //rightsModel.setRoleID(record.id);
            aside.show();
        };

        vm.destroy = function() {
            vm.destWatch();
            aside = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("EntitiesInfoCtrl", [
            "$scope",
            "entitiesListAside",
            EntitiesInfoCtrl
        ]);
})(angular);
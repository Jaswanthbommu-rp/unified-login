//  UserMgmt Role Info Icon Controller

(function(angular, undefined) {
    "use strict";

    function EntitiesInfoCtrl($scope, aside, dataModel) {
        var vm = this;

        vm.init = function() {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.dataModel = dataModel;
        };

        vm.showAside = function(record) {
            dataModel.setData(record);
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
            "pmEntitiesAssignModel",
            EntitiesInfoCtrl
        ]);
})(angular);
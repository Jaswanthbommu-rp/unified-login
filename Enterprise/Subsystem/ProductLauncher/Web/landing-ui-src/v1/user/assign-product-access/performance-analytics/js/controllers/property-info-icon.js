//  Property Info Icon Controller

(function (angular, undefined) {
    "use strict";

    function PAPropertyInfoIconCtrl($scope, aside, propertiesModel) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.showAside = function (record) {
            propertiesModel.setName(record.groupName);
            propertiesModel.setPropertyGroupID(record.groupId);
            aside.show();
        };

        vm.destroy = function () {
            vm.destWatch();
            aside = undefined;
            propertiesModel = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("PAPropertyInfoIconCtrl", [
            "$scope",
            "paPropertiesListAside",
            "paPropertiesModel",
            PAPropertyInfoIconCtrl
        ]);
})(angular);

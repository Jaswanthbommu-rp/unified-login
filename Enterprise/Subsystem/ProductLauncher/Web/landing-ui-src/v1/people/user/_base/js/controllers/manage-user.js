
//  Manage User Controller

(function (angular) {
    "use strict";

    function ManageUserCtrl($scope, $location, manageUserModel, userModel, userStates) {
        var vm = this;

        vm.init = function () {
            vm.destroyWatch = $scope.$on("$destroy", vm.destroy);

            vm.manageUser = manageUserModel.init();

            var urlPath = $location.path();
            if(urlPath.indexOf("add") != -1) {
                manageUserModel.setState(userStates.ADD_USER);
            } else if(urlPath.indexOf("edit") != -1) {
                manageUserModel.setState(userStates.EDIT_USER);
            } else {
                manageUserModel.setState(userStates.VIEW_USER);
            }

            //TODO manage user types and the components that are available for them
            // vm.activeUserType = userModel.getUserType();
        };

        vm.destroy = function () {
            manageUserModel.reset();

            vm.destroyWatch();
            vm.destroyWatch = undefined;
            vm = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ManageUserCtrl", [
            "$scope",
            "$location",
            "manageUserModel",
            "userSessionModel",
            "userStates",
        	ManageUserCtrl
        ]);
})(angular);

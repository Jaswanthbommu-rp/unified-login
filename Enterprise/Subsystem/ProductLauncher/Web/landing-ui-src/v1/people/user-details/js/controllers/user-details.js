//  UserDetails Controller

(function (angular) {
    "use strict";

    function UserDetailsCtrl($timeout, $stateParams, breadcrumbs) {
        var vm = this;

        var userId = $stateParams.userId;

        vm.init = function () {
        	vm.updateBreadcrumbs({"name": "John Smith"});
            vm.activePanels = [0];
        };

        vm.updateBreadcrumbs = function (data) {
        	$timeout(function() {
	            breadcrumbs.setActivePage({
	                text: data.name
	            });
        	});

            return vm;
        };

        vm.destroy = function () {
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("UserDetailsCtrl", [
        	"$timeout",
        	"$stateParams",
        	"rpBreadcrumbsModel",
        	UserDetailsCtrl
        ]);
})(angular);

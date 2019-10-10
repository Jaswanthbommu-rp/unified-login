//  Leasing & Rents Settings Controller

(function (angular) {
    "use strict";

    function LrcCtrl(navData, tabsMenuModel) {
        var vm = this,
            tabsMenu = tabsMenuModel();

        vm.init = function () {
            vm.tabsMenu = tabsMenu.setData(navData);
        };

        vm.destroy = function () {

        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("LrcCtrl", [
            "lrcSettingsNavData",
            "rpScrollingTabsMenuModel",
            LrcCtrl
        ]);
})(angular);

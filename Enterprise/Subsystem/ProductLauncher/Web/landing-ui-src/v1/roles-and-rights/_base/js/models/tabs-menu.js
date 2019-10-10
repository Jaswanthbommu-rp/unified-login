//  Roels and Rights Tabs Menu Model

(function(angular, undefined) {
    "use strict";

    function factory(tabsMenuModel, tabsData) {
        return tabsMenuModel().setData(tabsData.getList());
    }

    angular
        .module("settings")
        .factory("rolesRightsTabsMenu", [
            "rpScrollingTabsMenuModel",
            "rolesRightsTabsData",
            factory
        ]);
})(angular);
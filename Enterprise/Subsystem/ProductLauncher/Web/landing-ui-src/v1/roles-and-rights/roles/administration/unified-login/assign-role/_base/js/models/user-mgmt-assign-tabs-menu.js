//  New Role Tabs Menu

(function(angular, undefined) {
    "use strict";

    function factory(tabsMenuModel, tabsData) {
        var model = tabsMenuModel();

        model.setData(tabsData.getList()).disableUnsavedChangesCheck();

        return model;
    }

    angular
        .module("settings")
        .factory("userMgmtAssignRoleTabsMenu", [
            "rpScrollingTabsMenuModel",
            "userMgmtAssignTabsData",
            factory
        ]);
})(angular);
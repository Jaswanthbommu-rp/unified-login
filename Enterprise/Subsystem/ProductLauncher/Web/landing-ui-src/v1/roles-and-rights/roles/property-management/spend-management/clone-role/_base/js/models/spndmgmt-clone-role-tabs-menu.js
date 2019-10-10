//  Clone Role Tabs Menu

(function(angular, undefined) {
    "use strict";

    function factory(tabsMenuModel, tabsData) {
        var model = tabsMenuModel();

        model.setData(tabsData.getList()).disableUnsavedChangesCheck();

        return model;
    }

    angular
        .module("settings")
        .factory("spndmgmtCloneRoleTabsMenu", [
            "rpScrollingTabsMenuModel",
            "spndMgmtCloneTabsData",
            factory
        ]);
})(angular);
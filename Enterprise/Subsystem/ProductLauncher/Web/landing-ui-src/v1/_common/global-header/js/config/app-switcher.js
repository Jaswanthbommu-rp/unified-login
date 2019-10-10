//  Init App Switcher

(function (angular) {
    "use strict";

    function config(menuModel) {
        menuModel.setTabsText({
            favorites: "Favorites",
            families: "All Products"
        });

        menuModel.setManageLink({
            url: "#/products",
            text: "Manage Products"
        });
    }

    angular
        .module("settings")
        .run(["rpGhAppSwitcherMenuModel", config]);
})(angular);

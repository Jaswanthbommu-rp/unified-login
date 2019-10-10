//  Leasing & Rents Settings Nav Model

(function (angular) {
    "use strict";

    function factory() {
        return [{
            id: "01",
            href: "#/product/lrc",
            stateName: "product.lrc.settings",
            className: "",
            isActive: true,
            text: "Settings"
        }, {
            id: "02",
            href: "#/product/lrc/users",
            stateName: "product.lrc.users",
            className: "",
            isActive: false,
            text: "Users"
        }, {
            id: "03",
            href: "#/product/lrc/activity",
            stateName: "product.lrc.activity",
            className: "",
            isActive: false,
            text: "Activity"
        }];
    }

    angular
        .module("settings")
        .factory("lrcSettingsNavData", [factory]);
})(angular);

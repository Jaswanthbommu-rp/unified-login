//  SideMenuConfig Model

(function (angular, undefined) {
    "use strict";

    function factory(sideMenuModel) {
        function SideMenuConfig() {
            var s = this;
            s.init();
        }

        var p = SideMenuConfig.prototype;

        p.init = function () {
            var s = this;
            s.sideMenu = sideMenuModel();

            var list = [
                {
                    active: true,
                    text: "Production",
                    templateUrl: "user/assign-product-access/onesite/templates/onesite-production.html"
                },
                {
                    active: false,
                    text: "UAT",
                    templateUrl: "user/assign-product-access/onesite/templates/onesite-uat.html"
                }
            ];

            s.list = list;
            s.sideMenu.setList(list);
        };

        p.getIncUrl = function () {
            var s = this,
                activeItem = s.sideMenu.getActiveItem();

            return activeItem ? activeItem.templateUrl : "";
        };

        p.reset = function () {
            var s = this;
            s.sideMenu.setActive(s.list[0]);
        };

        return new SideMenuConfig();
    }

    angular
        .module("settings")
        .factory("sideMenuConfig", ["rpSideMenuModel", factory]);
})(angular);

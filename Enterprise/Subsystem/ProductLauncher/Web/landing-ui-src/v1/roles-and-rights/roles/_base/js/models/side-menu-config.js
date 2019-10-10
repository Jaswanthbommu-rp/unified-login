//  side menu Model

(function(angular, undefined) {
    "use strict";

    function factory(sideMenuModel) {
        function SolSideMenuModel() {
            var s = this;
            s.init();
        }

        var p = SolSideMenuModel.prototype;

        p.init = function() {
            var s = this;
            s.sideMenu = sideMenuModel();

            var list = [];

            s.list = list;
            s.sideMenu.setList(list);
        };

        p.setList = function(list) {
            var s = this;
            s.sideMenu.setList(list);
        };

        p.setSrc = function(src) {
            var s = this;
            s.sideMenu.setSrc(src);
            return s;
        };

        p.getIncUrl = function() {
            var s = this,
                activeItem = s.sideMenu.getActiveItem();
            return activeItem ? activeItem.templateUrl : "";
        };

        p.subscribe = function() {
            var s = this;
            return s.sideMenu.subscribe.apply(s.sideMenu, arguments);
        };

        p.reset = function() {
            var s = this;
        };

        return function() {
            return new SolSideMenuModel();
        };
    }

    angular
        .module("settings")
        .factory("solSideMenuModel", ["rpSideMenuModel", factory]);
})(angular);
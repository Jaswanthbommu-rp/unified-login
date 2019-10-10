//  Page Layout Service

(function (angular, undefined) {
    "use strict";

    function AppLayout(cookie, model) {
        var svc = this;

        svc.init = function () {
            var list,
                hide = cookie.read("crossover") === "True";

            list = [
                "appNav",
                "appHeader",
                "appFooter"
            ];

            model[hide ? "hide" : "show"](list);

            return true;
        };
    }

    angular
        .module("settings")
        .service("appLayoutSvc", [
            "rpCookie",
            "appLayoutModel",
            AppLayout
        ]);
})(angular);

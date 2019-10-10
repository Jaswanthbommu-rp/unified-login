//  Manage Profile Aside

(function (angular, undefined) {
    "use strict";

    function factory(asideModal) {
        var modalData = {
            keyboard: false,
            backdrop: "static",
            templateUrl: "manage-profile/base/templates/index.html"
        };

        return asideModal().setData(modalData);
    }

    angular
        .module("settings")
        .factory("mpAside", ["rpAsideModal", factory]);
})(angular);

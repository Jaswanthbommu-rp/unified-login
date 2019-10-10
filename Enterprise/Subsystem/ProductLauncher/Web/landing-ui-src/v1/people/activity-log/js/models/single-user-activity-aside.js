//  RIghts List Aside

(function (angular, undefined) {
    "use strict";

    function factory(asideModal) {
        var modalData = {
            keyboard: false,
            backdrop: "static",
            templateUrl: "people/activity-log/templates/single-user-activity-aside.html"
    };

        return asideModal().setData(modalData);
    }

    angular
        .module("settings")
        .factory("alSingleUserActivityAside", ["rpAsideModal", factory]);
})(angular);

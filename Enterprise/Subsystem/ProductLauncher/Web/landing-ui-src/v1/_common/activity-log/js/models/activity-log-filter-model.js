// Activity Log Filter data

(function (angular) {
    "use strict";

    function factory() {
        return function () {
            return {
                keyword: "",
                activity: "",
                daterange: "1-CM",
                sortby: "ApplicationTimeStamp-DESC"
            };
        };
    }

    angular
        .module("settings")
        .factory("activityLogFilterData", [factory]);
})(angular);

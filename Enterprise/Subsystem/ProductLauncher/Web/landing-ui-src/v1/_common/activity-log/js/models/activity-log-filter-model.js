// Activity Log Filter data

(function (angular) {
    "use strict";

    function factory(moment) {
        return function () {
            return {
                keyword: "",
                activity: "",
                daterange: "1-CM",
                startDate: moment().startOf('month'),
                endDate:moment(),
                sortby: "ApplicationTimeStamp-DESC"
            };
        };
    }

    angular
        .module("settings")
        .factory("activityLogFilterData", [
            "moment",
            factory
        ]);
})(angular);

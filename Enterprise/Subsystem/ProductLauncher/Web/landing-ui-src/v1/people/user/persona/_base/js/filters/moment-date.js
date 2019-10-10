(function (angular) {
    "use strict";

    function momentFilter(moment) {
        return function (date, format) {
            format = format || "MM/D/YY";

            if(angular.isUndefined(date) || date === null) {
                return "--";
            } else if(moment.isMoment(date)) {
                return date.format(format);
            }

            return date;
        };
    }

    angular
        .module("settings")
        .filter("momentDate", [
            "moment",
            momentFilter
        ]);
})(angular);
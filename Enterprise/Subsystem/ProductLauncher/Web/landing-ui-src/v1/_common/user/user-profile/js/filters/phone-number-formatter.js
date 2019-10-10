(function (angular) {
    "use strict";

    function filter() {
        return function (number) {
            if(angular.isUndefined(number) || number === null || number.length === 0) {
                return "";
            }

            var phoneNum = number.toString().trim(),
                start = phoneNum.slice(0, phoneNum.length - 4),
                tail = phoneNum.slice(phoneNum.length - 4),
                counter = 0,
                formatted = [];

            for(var i = start.length-1; i >= 0; i--) {
                var char = start[i];
                if(counter % 3 === 0) {
                    formatted.push(" ");
                }
                formatted.push(char);
                counter++;
            }

            return formatted.reverse().join("") + " " + tail;
        };
    }

    angular
        .module("settings")
        .filter("phoneNumber", [
            filter
        ]);
})(angular);
 
//  Focus If Directive

(function (angular) {
    "use strict";

    function focusIf($timeout) {
        return {
            restrict: 'A',
            scope: {
                focusIf:'='
            },
            link: function (scope, elem, attrs) {
                if (scope.focusIf) {
                    $timeout(function () {
                        elem.focus();
                    }, 100);
                }
            }
        };
    }

    angular
        .module("identity")
        .directive("focusIf", [
            "$timeout",
            focusIf
        ]);
})(angular);

//  Anti-Forgery Token Directive

(function (angular) {
    "use strict";

    function antiForgeryToken() {
        return {
            restrict: 'E',
            replace: true,
            scope: {
                token: "="
            },
            template: "<input type='hidden' name='{{token.name}}' value='{{token.value}}'>"
        };
    }

    angular
        .module("identity")
        .directive("antiForgeryToken", [
            antiForgeryToken
        ]);
})(angular);

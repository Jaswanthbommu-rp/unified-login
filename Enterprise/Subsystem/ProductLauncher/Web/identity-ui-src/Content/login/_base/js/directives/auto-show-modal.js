(function (angular, jQuery) {
    "use strict";

    function autoDisplayModalDirective() {
        return {
            link: function(scope, elem, attrs, ctrl) {
                jQuery(elem).find(".modal").modal("show");
            },
            restrict: "A"
        };
    }

    angular
        .module("identity")
        .directive("autoDisplayModal", [
            autoDisplayModalDirective
        ]);
})(angular, $); 

//  Access Not Defined Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function AccessNotDefinedModel() {
            var s = this;
            s.init();
        }

        var p = AccessNotDefinedModel.prototype;

        p.init = function () {
            var s = this;
            s.list = [];
        };

        p.setList = function (list) {
            var s = this;
            s.list = list;
            return s;
        };

        p.reset = function () {
            var s = this;
            s.list = [];
        };

        return new AccessNotDefinedModel();
    }

    angular
        .module("settings")
        .factory("accessNotDefinedModel", [factory]);
})(angular);

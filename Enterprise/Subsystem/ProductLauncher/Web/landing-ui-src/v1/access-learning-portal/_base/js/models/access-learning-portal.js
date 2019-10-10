//  modelTitleName Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function AlpModalStates() {
            var s = this;
            s.init();
        }

        var p = AlpModalStates.prototype;

        p.init = function () {
            var s = this;
            s.data = {
                prevAcct: true,
                enterEmail: false,
                notFound: false,
                notCreated: false
            };
            s.modalOrder = {
                start: "prevAcct",
                prevAcct: "enterEmail",
                enterEmail: "notFound",
                notFound: "notCreated",
                notCreated: ""
            };
            s.dataCopy = angular.copy(s.data);
        };

        p.showModal = function (modalName) {
            var s = this;
            angular.forEach(s.data, function (val, key) {
                s.data[key] = key == modalName;
            });
            return s;
        };

        p.getActiveModal = function () {
            var activeModal = "start",
                s = this;
            angular.forEach(s.data, function (val, key) {
                activeModal = s.data[key] === true ? key : activeModal;
            });
            return activeModal;
        };

        p.getNextModal = function () {
            var s = this;
            var currModal = s.getActiveModal();
            return s.modalOrder[currModal];
        };

        p.reset = function () {
            var s = this;
            s.data = angular.copy(s.dataCopy);
        };

        return new AlpModalStates();
    }

    angular
        .module("settings")
        .factory("alpModalStates", [factory]);
})(angular);

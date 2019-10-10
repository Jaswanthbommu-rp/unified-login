//  modelTitleName Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function ELMSModalStates() {
            var s = this;
            s.init();
        }

        var p = ELMSModalStates.prototype;

        p.init = function () {
            var s = this;
            s.data = {
                haveUser: true,
                enterEmail: false,
                notFound: false,
                contactAdmin: false
            };
            s.modalOrder = {
                start: "haveUser",
                haveUser: "enterEmail",
                enterEmail: "notFound",
                notFound: ""
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

        return new ELMSModalStates();
    }

    angular
        .module("settings")
        .factory("elmsModalStates", [factory]);
})(angular);
